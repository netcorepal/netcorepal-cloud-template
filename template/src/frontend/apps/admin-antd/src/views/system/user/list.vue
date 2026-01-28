<script lang="ts" setup>
import type { Recordable } from '@vben/types';

import type { OnActionClickParams } from '#/adapter/vxe-table';
import type { SystemUserApi } from '#/api/system/user';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { Plus } from '@vben/icons';

import { Button, message, Modal } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deleteUser, getUserList, updateUser } from '#/api/system/user';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid<SystemUserApi.SystemUser>({
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
  gridOptions: {
    columns: useColumns(onActionClick, onStatusChange),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async ({ page }: { page: { currentPage: number; pageSize: number } }, formValues: Recordable<any>) => {
          const result = await getUserList({
            pageIndex: page.currentPage, // 后端期望 pageIndex（从1开始），而不是 page
            pageSize: page.pageSize,
            countTotal: true, // 需要总数用于分页显示
            ...formValues,
          });
          // vxe-table 根据全局配置 response: { result: 'items', total: 'total' } 读取数据
          return {
            items: result.items,
            total: result.total,
          };
        },
      },
    },
    rowConfig: {
      keyField: 'userId',
    },

    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: true,
    },
  },
});

function onActionClick(e: OnActionClickParams<SystemUserApi.SystemUser>) {
  switch (e.code) {
    case 'delete': {
      onDelete(e.row);
      break;
    }
    case 'edit': {
      onEdit(e.row);
      break;
    }
  }
}

/**
 * 将Antd的Modal.confirm封装为promise，方便在异步函数中调用。
 * @param content 提示内容
 * @param title 提示标题
 */
function confirm(content: string, title: string) {
  return new Promise((resolve, reject) => {
    Modal.confirm({
      content,
      onCancel() {
        reject(new Error('已取消'));
      },
      onOk() {
        resolve(true);
      },
      title,
    });
  });
}

/**
 * 状态开关即将改变
 * @param newStatus 期望改变的状态值（0或1）
 * @param row 行数据
 * @returns 返回false则中止改变，返回其他值（undefined、true）则允许改变
 */
async function onStatusChange(
  newStatus: 0 | 1,
  row: SystemUserApi.SystemUser,
) {
  const status: Recordable<string> = {
    0: '禁用',
    1: '启用',
  };
  try {
    await confirm(
      `你要将${row.name}的状态切换为 【${status[newStatus.toString()]}】 吗？`,
      `切换状态`,
    );
    // 通过更新用户信息来切换状态
    await updateUser(row.userId, {
      name: row.name,
      email: row.email,
      phone: row.phone || '',
      realName: row.realName || '',
      status: newStatus,
      gender: row.gender || '',
      age: row.age || 0,
      birthDate: row.birthDate,
      deptId: row.deptId || '0',
      deptName: row.deptName || '',
      password: '', // 不更新密码
    });
    // 刷新列表以获取最新状态
    onRefresh();
    return true;
  } catch {
    return false;
  }
}

function onEdit(row: SystemUserApi.SystemUser) {
  formDrawerApi.setData(row).open();
}

function onDelete(row: SystemUserApi.SystemUser) {
  const hideLoading = message.loading({
    content: $t('ui.actionMessage.deleting', [row.name]),
    duration: 0,
    key: 'action_process_msg',
  });
  deleteUser(row.userId)
    .then(() => {
      message.success({
        content: $t('ui.actionMessage.deleteSuccess', [row.name]),
        key: 'action_process_msg',
      });
      onRefresh();
    })
    .catch(() => {
      hideLoading();
    });
}

function onRefresh() {
  gridApi.query();
}

function onCreate() {
  formDrawerApi.setData({}).open();
}
</script>
<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid :table-title="$t('system.user.list')">
      <template #toolbar-tools>
        <Button type="primary" @click="onCreate">
          <Plus class="size-5" />
          {{ $t('ui.actionTitle.create', [$t('system.user.name')]) }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
