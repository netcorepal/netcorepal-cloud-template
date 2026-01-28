<script lang="ts" setup>
import type { SystemUserApi } from '#/api/system/user';

import { computed, nextTick, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { message } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createUser, updateUser, updateUserRoles } from '#/api/system/user';
import { getDeptTree } from '#/api/system/dept';
import { getRoleList } from '#/api/system/role';
import { $t } from '#/locales';

import { useFormSchema } from '../data';

/**
 * 从部门树中查找部门名称
 */
function findDeptName(deptTree: any[], deptId: string): string {
  for (const dept of deptTree) {
    if (dept.id === deptId) {
      return dept.name;
    }
    if (dept.children && dept.children.length > 0) {
      const found = findDeptName(dept.children, deptId);
      if (found) {
        return found;
      }
    }
  }
  return '';
}

const emits = defineEmits(['success']);

const formData = ref<SystemUserApi.SystemUser>();
const id = ref<string>();

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: useFormSchema(),
  showDefaultActions: false,
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    drawerApi.lock();
    try {
      // 获取部门名称（如果选择了部门）
      let deptName = values.deptName || '';
      if (values.deptId && !deptName) {
        const deptTree = await getDeptTree();
        deptName = findDeptName(deptTree, values.deptId);
      }

      if (id.value) {
        // 更新用户
        await updateUser(id.value, {
          name: values.name,
          email: values.email,
          phone: values.phone || '',
          realName: values.realName,
          status: values.status ?? 1,
          gender: values.gender || '',
          age: 0, // 后端会根据birthDate自动计算
          birthDate: values.birthDate,
          deptId: values.deptId || '0',
          deptName: deptName,
          password: values.password || '', // 为空则不更新密码
        });

        // 更新用户角色（如果角色有变化）
        if (values.roleIds && Array.isArray(values.roleIds)) {
          await updateUserRoles(id.value, values.roleIds);
        }
      } else {
        // 创建用户 - 验证密码必填
        if (!values.password || values.password.trim() === '') {
          message.error($t('ui.formRules.required', [$t('system.user.password')]));
          drawerApi.unlock();
          return;
        }
        await createUser({
          name: values.name,
          email: values.email,
          password: values.password,
          phone: values.phone || '',
          realName: values.realName,
          status: values.status ?? 1,
          gender: values.gender || '',
          birthDate: values.birthDate,
          deptId: values.deptId,
          deptName: deptName,
          roleIds: values.roleIds || [],
        });
      }
      emits('success');
      drawerApi.close();
    } catch {
      drawerApi.unlock();
    }
  },

  async onOpenChange(isOpen) {
    if (isOpen) {
      const data = drawerApi.getData<SystemUserApi.SystemUser>();
      formApi.resetForm();

      if (data && data.userId) {
        formData.value = data;
        id.value = data.userId;
      } else {
        id.value = undefined;
        formData.value = undefined;
      }

      // Wait for Vue to flush DOM updates (form fields mounted)
      await nextTick();
      if (data && data.userId) {
        // 编辑模式：需要将角色名称转换为角色ID
        let roleIds: string[] = [];
        if (data.roles && data.roles.length > 0) {
          // 获取所有角色列表
          const roleListResult = await getRoleList({
            pageIndex: 1,
            pageSize: 1000,
            countTotal: false,
          });
          // 根据角色名称查找对应的角色ID
          const roleMap = new Map(
            roleListResult.items.map((role) => [role.name, role.roleId]),
          );
          roleIds = data.roles
            .map((roleName) => roleMap.get(roleName))
            .filter((id): id is string => !!id);
        }

        // 编辑模式：设置表单值
        formApi.setValues({
          name: data.name,
          email: data.email,
          phone: data.phone || '',
          realName: data.realName || '',
          status: data.status ?? 1,
          gender: data.gender || '',
          birthDate: data.birthDate,
          deptId: data.deptId || undefined,
          deptName: data.deptName || '',
          roleIds: roleIds,
          password: '', // 编辑时不显示密码
        });
      } else {
        // 创建模式：设置默认值
        formApi.setValues({
          status: 1,
        });
      }
    }
  },
});

const getDrawerTitle = computed(() => {
  return formData.value?.userId
    ? $t('common.edit', [$t('system.user.name')])
    : $t('common.create', [$t('system.user.name')]);
});
</script>
<template>
  <Drawer :title="getDrawerTitle">
    <Form />
  </Drawer>
</template>
