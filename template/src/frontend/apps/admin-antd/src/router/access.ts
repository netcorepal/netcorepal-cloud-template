import type {
  ComponentRecordType,
  GenerateMenuAndRoutesOptions,
} from '@vben/types';

import { generateAccessible } from '@vben/access';
import { preferences } from '@vben/preferences';

import { BasicLayout, IFrameView } from '#/layouts';

const forbiddenComponent = () => import('#/views/_core/fallback/forbidden.vue');

async function generateAccess(options: GenerateMenuAndRoutesOptions) {
  const pageMap: ComponentRecordType = import.meta.glob('../views/**/*.vue');

  const layoutMap: ComponentRecordType = {
    BasicLayout,
    IFrameView,
  };

  // 使用前端访问控制模式，不再调用后端菜单接口
  // 所有路由通过静态配置（router/routes/modules/*.ts）定义
  // 权限控制通过 meta.authority 中的 PermissionCodes 实现
  return await generateAccessible(preferences.app.accessMode, {
    ...options,
    // 移除 fetchMenuListAsync，不再调用菜单 API
    // 可以指定没有权限跳转403页面
    forbiddenComponent,
    // 如果 route.meta.menuVisibleWithForbidden = true
    layoutMap,
    pageMap,
  });
}

export { generateAccess };
