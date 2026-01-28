<script lang="ts" setup>
import type { PermissionTreeNode } from '#/utils/permission-tree';
import type { SystemRoleApi } from '#/api/system/role';

import { computed, nextTick, ref } from 'vue';

import { Tree, useVbenDrawer } from '@vben/common-ui';
import { IconifyIcon } from '@vben/icons';

import { Spin } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { createRole, updateRole } from '#/api/system/role';
import { buildPermissionTree } from '#/utils/permission-tree';
import { $t } from '#/locales';

import { useFormSchema } from '../data';

const emits = defineEmits(['success']);

const formData = ref<SystemRoleApi.SystemRole>();

const [Form, formApi] = useVbenForm({
  schema: useFormSchema(),
  showDefaultActions: false,
});

const permissions = ref<PermissionTreeNode[]>([]);

const id = ref<string>();
const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    drawerApi.lock();
    try {
      if (id.value) {
        await updateRole(id.value, {
          name: values.name,
          description: values.description || '',
          permissionCodes: values.permissionCodes || [],
        });
      } else {
        await createRole({
          name: values.name,
          description: values.description || '',
          permissionCodes: values.permissionCodes || [],
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
      const data = drawerApi.getData<SystemRoleApi.SystemRole>();
      formApi.resetForm();

      if (data && data.roleId) {
        formData.value = data;
        id.value = data.roleId;
      } else {
        id.value = undefined;
        formData.value = undefined;
      }

      // 加载权限树数据（使用静态数据，不需要 API 调用）
      if (permissions.value.length === 0) {
        permissions.value = buildPermissionTree();
      }

      // Wait for Vue to flush DOM updates (form fields mounted)
      await nextTick();
      if (data && data.roleId) {
        formApi.setValues({
          name: data.name,
          description: data.description || '',
          isActive: data.isActive,
          permissionCodes: data.permissionCodes || [],
        });
      }
    }
  },
});

const getDrawerTitle = computed(() => {
  return formData.value?.roleId
    ? $t('common.edit', [$t('system.role.name')])
    : $t('common.create', [$t('system.role.name')]);
});
</script>
<template>
  <Drawer :title="getDrawerTitle">
    <Form>
      <template #permissionCodes="slotProps">
        <Spin :spinning="false" wrapper-class-name="w-full">
          <Tree
            :tree-data="permissions"
            multiple
            bordered
            :default-expanded-level="2"
            v-bind="slotProps"
            value-field="value"
            label-field="label"
            icon-field="icon"
          >
            <template #node="{ value }">
              <IconifyIcon v-if="value.icon" :icon="value.icon" />
              {{ value.label }}
            </template>
          </Tree>
        </Spin>
      </template>
    </Form>
  </Drawer>
</template>
<style lang="css" scoped>
:deep(.ant-tree-title) {
  .tree-actions {
    display: none;
    margin-left: 20px;
  }
}

:deep(.ant-tree-title:hover) {
  .tree-actions {
    display: flex;
    flex: auto;
    justify-content: flex-end;
    margin-left: 20px;
  }
}
</style>