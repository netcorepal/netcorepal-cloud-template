import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn, VxeTableGridOptions } from '#/adapter/vxe-table';
import type { SystemUserApi } from '#/api/system/user';

import { z } from '#/adapter/form';
import { getDeptTree } from '#/api/system/dept';
import { getRoleList } from '#/api/system/role';
import { $t } from '#/locales';

/**
 * 获取所有角色列表（用于下拉选择）
 */
async function getAllRolesForSelect() {
  const result = await getRoleList({
    pageIndex: 1,
    pageSize: 1000, // 获取所有角色
    countTotal: false,
  });
  return result.items.map((role) => ({
    label: role.name,
    value: role.roleId,
  }));
}

/**
 * 获取编辑表单的字段配置
 */
export function useFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'name',
      label: $t('system.user.userName'),
      rules: 'required',
    },
    {
      component: 'Input',
      fieldName: 'email',
      label: $t('system.user.email'),
      rules: z.string().email($t('ui.formRules.email')),
    },
    {
      component: 'Input',
      fieldName: 'phone',
      label: $t('system.user.phone'),
      rules: z
        .string()
        .regex(/^1[3-9]\d{9}$/, $t('ui.formRules.phone'))
        .optional(),
    },
    {
      component: 'Input',
      fieldName: 'realName',
      label: $t('system.user.realName'),
      rules: 'required',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('system.user.male'), value: '男' },
          { label: $t('system.user.female'), value: '女' },
          { label: $t('system.user.other'), value: '其他' },
        ],
      },
      fieldName: 'gender',
      label: $t('system.user.gender'),
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD',
        valueFormat: 'YYYY-MM-DD',
      },
      fieldName: 'birthDate',
      label: $t('system.user.birthDate'),
    },
    {
      component: 'ApiTreeSelect',
      componentProps: {
        allowClear: true,
        api: getDeptTree,
        class: 'w-full',
        labelField: 'name',
        valueField: 'id',
        childrenField: 'children',
      },
      fieldName: 'deptId',
      label: $t('system.user.dept'),
    },
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: getAllRolesForSelect,
        class: 'w-full',
        labelField: 'label',
        valueField: 'value',
        mode: 'multiple',
      },
      fieldName: 'roleIds',
      label: $t('system.user.roles'),
    },
    {
      component: 'RadioGroup',
      componentProps: {
        buttonStyle: 'solid',
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
        optionType: 'button',
      },
      defaultValue: 1,
      fieldName: 'status',
      label: $t('system.user.status'),
    },
    {
      component: 'InputPassword',
      fieldName: 'password',
      label: $t('system.user.password'),
      rules: z.string().min(6, $t('ui.formRules.minLength', [$t('system.user.password'), 6])).optional(),
    },
  ];
}

/**
 * 获取列表搜索表单配置
 */
export function useGridFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'keyword',
      label: $t('system.user.keyword'),
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: true,
        options: [
          { label: $t('common.enabled'), value: 1 },
          { label: $t('common.disabled'), value: 0 },
        ],
      },
      fieldName: 'status',
      label: $t('system.user.status'),
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns<T = SystemUserApi.SystemUser>(
  onActionClick: OnActionClickFn<T>,
  onStatusChange?: (
    newStatus: any,
    row: T,
  ) => PromiseLike<boolean | undefined>,
): VxeTableGridOptions['columns'] {
  return [
    {
      field: 'name',
      title: $t('system.user.userName'),
      width: 150,
    },
    {
      field: 'realName',
      title: $t('system.user.realName'),
      width: 120,
    },
    {
      field: 'email',
      title: $t('system.user.email'),
      width: 200,
    },
    {
      field: 'phone',
      title: $t('system.user.phone'),
      width: 120,
    },
    {
      field: 'gender',
      title: $t('system.user.gender'),
      width: 80,
    },
    {
      field: 'age',
      title: $t('system.user.age'),
      width: 80,
    },
    {
      field: 'deptName',
      title: $t('system.user.dept'),
      width: 150,
    },
    {
      field: 'roles',
      title: $t('system.user.roles'),
      minWidth: 200,
      formatter: ({ cellValue }) => {
        if (Array.isArray(cellValue)) {
          return cellValue.join(', ');
        }
        return cellValue || '';
      },
    },
    {
      cellRender: {
        attrs: { beforeChange: onStatusChange },
        name: onStatusChange ? 'CellSwitch' : 'CellTag',
      },
      field: 'status',
      title: $t('system.user.status'),
      width: 100,
    },
    {
      field: 'createdAt',
      formatter: 'formatDateTime',
      title: $t('system.user.createTime'),
      width: 180,
    },
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          nameTitle: $t('system.user.name'),
          onClick: onActionClick,
        },
        name: 'CellOperation',
      },
      field: 'operation',
      fixed: 'right',
      title: $t('system.user.operation'),
      width: 130,
    },
  ];
}
