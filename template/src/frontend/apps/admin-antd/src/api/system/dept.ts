import { requestClient } from '#/api/request';

export namespace SystemDeptApi {
  export interface SystemDept {
    [key: string]: any;
    children?: SystemDept[];
    id: string;
    name: string;
    remark?: string;
    parentId?: string;
    status: 0 | 1;
    createdAt: string;
  }
}

/**
 * 获取部门列表数据
 */
async function getDeptList() {
  return requestClient.get<Array<SystemDeptApi.SystemDept>>('/dept');
}

/**
 * 获取部门树数据
 */
async function getDeptTree() {
  return requestClient.get<Array<SystemDeptApi.SystemDept>>('/dept/tree');
}

/**
 * 获取单个部门信息
 * @param id 部门 ID
 */
async function getDept(id: string) {
  return requestClient.get<SystemDeptApi.SystemDept>(`/dept/${id}`);
}

/**
 * 创建部门
 * @param data 部门数据
 */
async function createDept(data: {
  name: string;
  remark?: string;
  parentId?: string;
  status: 0 | 1;
}) {
  return requestClient.post('/dept', data);
}

/**
 * 更新部门
 *
 * @param id 部门 ID
 * @param data 部门数据
 */
async function updateDept(
  id: string,
  data: {
    name: string;
    remark?: string;
    parentId?: string;
    status: 0 | 1;
  },
) {
  return requestClient.put('/dept', {
    id,
    ...data,
  });
}

/**
 * 删除部门
 * @param id 部门 ID
 */
async function deleteDept(id: string) {
  return requestClient.delete(`/dept/${id}`);
}

export {
  createDept,
  deleteDept,
  getDept,
  getDeptList,
  getDeptTree,
  updateDept,
};
