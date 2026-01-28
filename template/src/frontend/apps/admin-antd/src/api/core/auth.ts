import { baseRequestClient, requestClient } from '#/api/request';

export namespace AuthApi {
  /** 登录接口参数 */
  export interface LoginParams {
    password?: string;
    username?: string;
  }

  /** 登录接口返回值 */
  export interface LoginResult {
    token: string;
    refreshToken: string;
    userId: string;
    name: string;
    email: string;
    roles: string;
    permissionCodes: string[]; // 权限代码列表
    tokenExpiryTime: string;
  }

  export interface RefreshTokenResult {
    data: string;
    status: number;
  }
}

/**
 * 登录
 */
export async function loginApi(data: AuthApi.LoginParams) {
  return requestClient.post<AuthApi.LoginResult>('/user/login', data);
}

/**
 * 刷新accessToken
 */
export async function refreshTokenApi() {
  return baseRequestClient.post<AuthApi.RefreshTokenResult>('/auth/refresh', {
    withCredentials: true,
  });
}

/**
 * 退出登录
 */
export async function logoutApi() {
  return requestClient.post('/auth/logout', {
    withCredentials: true,
  });
}

/**
 * 获取用户权限码
 * 注意：如果后端没有此接口，将返回空数组
 */
export async function getAccessCodesApi() {
  try {
    return await requestClient.get<string[]>('/auth/codes');
  } catch (error) {
    // 如果后端没有权限码接口，返回空数组
    console.warn('权限码接口不可用:', error);
    return [];
  }
}
