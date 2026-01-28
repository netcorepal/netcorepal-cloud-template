// 菜单 API 已移除，改为使用前端访问控制模式（accessMode: 'frontend'）
// 所有路由通过静态配置（router/routes/modules/*.ts）定义
// 权限控制通过 meta.authority 中的 PermissionCodes 实现
// 如需恢复菜单 API，请取消下面的注释

/*
import type { RouteRecordStringComponent } from '@vben/types';

import { requestClient } from '#/api/request';

export async function getAllMenusApi() {
  try {
    return await requestClient.get<RouteRecordStringComponent[]>('/menu/all');
  } catch (error) {
    console.warn('菜单接口不可用，使用静态路由:', error);
    return [];
  }
}
*/