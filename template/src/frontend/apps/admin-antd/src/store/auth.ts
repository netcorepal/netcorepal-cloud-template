import type { Recordable, UserInfo } from '@vben/types';

import { ref } from 'vue';
import { useRouter } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';
import { resetAllStores, useAccessStore, useUserStore } from '@vben/stores';

import { notification } from 'ant-design-vue';
import { defineStore } from 'pinia';

import { getAccessCodesApi, getUserInfoApi, loginApi, logoutApi } from '#/api';
import { $t } from '#/locales';

export const useAuthStore = defineStore('auth', () => {
  const accessStore = useAccessStore();
  const userStore = useUserStore();
  const router = useRouter();

  const loginLoading = ref(false);

  /**
   * 异步处理登录操作
   * Asynchronously handle the login process
   * @param params 登录表单数据
   */
  async function authLogin(
    params: Recordable<any>,
    onSuccess?: () => Promise<void> | void,
  ) {
    // 异步处理用户登录操作并获取 accessToken
    let userInfo: null | UserInfo = null;
    try {
      loginLoading.value = true;
      const loginResult = await loginApi(params);

      // 如果成功获取到 token
      if (loginResult.token) {
        accessStore.setAccessToken(loginResult.token);
        
        // 保存 userId 到 localStorage，以便后续使用
        if (loginResult.userId) {
          localStorage.setItem('userId', loginResult.userId);
        }

        // 获取用户信息并存储到 accessStore 中
        // 注意：需要从登录响应中获取 userId 来调用用户信息接口
        // 权限代码直接从登录响应中获取，无需额外调用 API
        userInfo = await fetchUserInfo(loginResult.userId);

        userStore.setUserInfo(userInfo);
        // 从登录响应中获取权限代码，如果没有则返回空数组
        accessStore.setAccessCodes(loginResult.permissionCodes || []);
        
        // 重置访问检查状态，确保路由守卫会重新生成菜单
        accessStore.setIsAccessChecked(false);

        if (accessStore.loginExpired) {
          accessStore.setLoginExpired(false);
        } else {
          onSuccess
            ? await onSuccess?.()
            : await router.push(
                userInfo.homePath || preferences.app.defaultHomePath,
              );
        }

        if (userInfo?.realName) {
          notification.success({
            description: `${$t('authentication.loginSuccessDesc')}:${userInfo?.realName}`,
            duration: 3,
            message: $t('authentication.loginSuccess'),
          });
        }
      }
    } finally {
      loginLoading.value = false;
    }

    return {
      userInfo,
    };
  }

  async function logout(redirect: boolean = true) {
    try {
      await logoutApi();
    } catch {
      // 不做任何处理
    }
    // 清除保存的 userId
    localStorage.removeItem('userId');
    resetAllStores();
    accessStore.setLoginExpired(false);

    // 回登录页带上当前路由地址
    await router.replace({
      path: LOGIN_PATH,
      query: redirect
        ? {
            redirect: encodeURIComponent(router.currentRoute.value.fullPath),
          }
        : {},
    });
  }

  async function fetchUserInfo(userId?: string) {
    let userInfo: null | UserInfo = null;
    try {
      // 如果没有提供 userId，尝试从 localStorage 获取
      const targetUserId = userId || localStorage.getItem('userId');
      
      if (targetUserId) {
        userInfo = await getUserInfoApi(targetUserId);
      } else {
        // 如果没有 userId，使用已存储的用户信息
        if (userStore.userInfo) {
          return userStore.userInfo;
        }
        // 如果都没有，返回默认值
        throw new Error('无法获取用户信息：缺少 userId');
      }
      if (userInfo) {
        userStore.setUserInfo(userInfo);
      }
    } catch (error) {
      // 如果获取用户信息失败，使用已存储的用户信息或返回默认值
      console.warn('获取用户信息失败:', error);
      if (userStore.userInfo) {
        return userStore.userInfo;
      }
      // 返回一个基本的用户信息对象，避免后续代码报错
      userInfo = {
        roles: [],
      } as UserInfo;
    }
    return userInfo;
  }

  function $reset() {
    loginLoading.value = false;
  }

  return {
    $reset,
    authLogin,
    fetchUserInfo,
    loginLoading,
    logout,
  };
});
