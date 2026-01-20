/**
 * 权限码常量定义
 * 与后端 PermissionCodes.cs 保持一致
 */
export const PermissionCodes = {
  // 角色管理权限
  RoleManagement: 'RoleManagement',
  RoleCreate: 'RoleCreate',
  RoleEdit: 'RoleEdit',
  RoleDelete: 'RoleDelete',
  RoleView: 'RoleView',
  RoleUpdatePermissions: 'RoleUpdatePermissions',

  // 用户管理权限
  UserManagement: 'UserManagement',
  UserCreate: 'UserCreate',
  UserEdit: 'UserEdit',
  UserDelete: 'UserDelete',
  UserView: 'UserView',
  UserRoleAssign: 'UserRoleAssign',
  UserResetPassword: 'UserResetPassword',

  // 部门管理权限
  DeptManagement: 'DeptManagement',
  DeptCreate: 'DeptCreate',
  DeptEdit: 'DeptEdit',
  DeptDelete: 'DeptDelete',
  DeptView: 'DeptView',

  // 所有接口访问权限
  AllApiAccess: 'AllApiAccess',
} as const;
