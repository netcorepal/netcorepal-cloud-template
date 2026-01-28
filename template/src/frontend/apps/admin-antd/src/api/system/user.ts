import type { Recordable } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace SystemUserApi {
  export interface SystemUser {
    [key: string]: any;
    userId: string;
    name: string;
    email: string;
    phone: string;
    realName: string;
    status: 0 | 1;
    gender: string;
    age: number;
    birthDate: string;
    deptId?: string;
    deptName?: string;
    roles: string[];
    createdAt: string;
  }
}

/**
 * 获取用户列表数据
 */
async function getUserList(params: Recordable<any>) {
  const result = await requestClient.get<{
    items: Array<{
      userId: string;
      name: string;
      email: string;
      phone: string;
      realName: string;
      status: 0 | 1;
      gender: string;
      age: number;
      birthDate: string;
      deptId?: string;
      deptName?: string;
      roles: string[];
      createdAt: string;
    }>;
    total: number;
    page: number;
    pageSize: number;
  }>('/users', { params });
  return result;
}

/**
 * 获取单个用户信息
 * @param id 用户 ID
 */
async function getUser(id: string) {
  return requestClient.get<SystemUserApi.SystemUser>(`/users/${id}`);
}

/**
 * 创建用户
 * @param data 用户数据
 */
async function createUser(data: {
  name: string;
  email: string;
  password: string;
  phone: string;
  realName: string;
  status: 0 | 1;
  gender: string;
  birthDate: string;
  deptId?: string;
  deptName?: string;
  roleIds: string[];
}) {
  return requestClient.post('/users', data);
}

/**
 * 更新用户
 *
 * @param id 用户 ID
 * @param data 用户数据
 */
async function updateUser(
  id: string,
  data: {
    name: string;
    email: string;
    phone: string;
    realName: string;
    status: 0 | 1;
    gender: string;
    age: number;
    birthDate: string;
    deptId: string;
    deptName: string;
    password?: string;
  },
) {
  return requestClient.put('/user/update', {
    userId: id,
    ...data,
  });
}

/**
 * 删除用户
 * @param id 用户 ID
 */
async function deleteUser(id: string) {
  return requestClient.delete(`/users/${id}`);
}

/**
 * 更新用户角色
 * @param userId 用户 ID
 * @param roleIds 角色ID列表
 */
async function updateUserRoles(userId: string, roleIds: string[]) {
  return requestClient.put('/users/update-roles', {
    userId,
    roleIds,
  });
}

export {
  createUser,
  deleteUser,
  getUser,
  getUserList,
  updateUser,
  updateUserRoles,
};
