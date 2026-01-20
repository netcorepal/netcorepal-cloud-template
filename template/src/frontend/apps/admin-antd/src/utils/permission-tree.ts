import type { DataNode } from 'ant-design-vue/es/tree';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

/**
 * 权限树节点接口
 */
export interface PermissionTreeNode extends DataNode {
  value: string;
  label: string;
  icon?: string;
  children?: PermissionTreeNode[];
}

/**
 * 构建权限树结构
 * 基于 PermissionDefinitionContext 的层级结构
 */
export function buildPermissionTree(): PermissionTreeNode[] {
  return [
    {
      value: PermissionCodes.UserManagement,
      label: '用户管理',
      icon: 'mdi:account',
      children: [
        {
          value: PermissionCodes.UserView,
          label: '查看用户',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.UserCreate,
          label: '创建用户',
          icon: 'mdi:account-plus',
        },
        {
          value: PermissionCodes.UserEdit,
          label: '编辑用户',
          icon: 'mdi:account-edit',
        },
        {
          value: PermissionCodes.UserDelete,
          label: '删除用户',
          icon: 'mdi:account-remove',
        },
        {
          value: PermissionCodes.UserRoleAssign,
          label: '分配用户角色',
          icon: 'mdi:account-group',
        },
        {
          value: PermissionCodes.UserResetPassword,
          label: '重置用户密码',
          icon: 'mdi:lock-reset',
        },
      ],
    },
    {
      value: PermissionCodes.RoleManagement,
      label: '角色管理',
      icon: 'mdi:account-group',
      children: [
        {
          value: PermissionCodes.RoleView,
          label: '查看角色',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.RoleCreate,
          label: '创建角色',
          icon: 'mdi:account-plus',
        },
        {
          value: PermissionCodes.RoleEdit,
          label: '编辑角色',
          icon: 'mdi:account-edit',
        },
        {
          value: PermissionCodes.RoleDelete,
          label: '删除角色',
          icon: 'mdi:account-remove',
        },
        {
          value: PermissionCodes.RoleUpdatePermissions,
          label: '更新角色权限',
          icon: 'mdi:shield-edit',
        },
      ],
    },
    {
      value: PermissionCodes.DeptManagement,
      label: '部门管理',
      icon: 'charm:organisation',
      children: [
        {
          value: PermissionCodes.DeptView,
          label: '查看部门',
          icon: 'mdi:eye',
        },
        {
          value: PermissionCodes.DeptCreate,
          label: '创建部门',
          icon: 'mdi:account-plus',
        },
        {
          value: PermissionCodes.DeptEdit,
          label: '编辑部门',
          icon: 'mdi:account-edit',
        },
        {
          value: PermissionCodes.DeptDelete,
          label: '删除部门',
          icon: 'mdi:account-remove',
        },
      ],
    },
    {
      value: PermissionCodes.AllApiAccess,
      label: '所有接口访问权限',
      icon: 'mdi:shield-check',
    },
  ];
}

/**
 * 获取所有权限码（扁平列表）
 */
export function getAllPermissionCodes(): string[] {
  const tree = buildPermissionTree();
  const codes: string[] = [];

  function traverse(nodes: PermissionTreeNode[]) {
    for (const node of nodes) {
      codes.push(node.value);
      if (node.children) {
        traverse(node.children);
      }
    }
  }

  traverse(tree);
  return codes;
}
