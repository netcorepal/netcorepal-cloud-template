import type { RouteRecordRaw } from 'vue-router';

import { PermissionCodes } from '#/constants/permission-codes';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ion:settings-outline',
      order: 9997,
      title: $t('system.title'),
      // 父路由需要任一权限即可显示
      authority: [PermissionCodes.RoleManagement, PermissionCodes.DeptManagement, PermissionCodes.UserManagement],
    },
    name: 'System',
    path: '/system',
    children: [
      {
        path: '/system/role',
        name: 'SystemRole',
        meta: {
          icon: 'mdi:account-group',
          title: $t('system.role.title'),
          authority: [PermissionCodes.RoleManagement], // 使用权限码控制访问
        },
        component: () => import('#/views/system/role/list.vue'),
      },
      {
        path: '/system/dept',
        name: 'SystemDept',
        meta: {
          icon: 'charm:organisation',
          title: $t('system.dept.title'),
          authority: [PermissionCodes.DeptManagement], // 使用权限码控制访问
        },
        component: () => import('#/views/system/dept/list.vue'),
      },
      {
        path: '/system/user',
        name: 'SystemUser',
        meta: {
          icon: 'mdi:account',
          title: $t('system.user.title'),
          authority: [PermissionCodes.UserManagement], // 使用权限码控制访问
        },
        component: () => import('#/views/system/user/list.vue'),
      },
    ],
  },
];

export default routes;
