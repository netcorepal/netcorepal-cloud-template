import type { UserInfo } from '@vben/types';

import { requestClient } from '#/api/request';

/**
 * 获取用户信息
 */
export async function getUserInfoApi(userId?: string) {
  if (userId) {
    return requestClient.get<UserInfo>(`/user/profile/${userId}`);
  }
  // 如果没有提供 userId，尝试调用接口（如果后端不支持，会抛出错误）
  // 注意：后端接口需要 userId，所以这里应该总是传入 userId
  throw new Error('需要提供 userId 参数');
}
