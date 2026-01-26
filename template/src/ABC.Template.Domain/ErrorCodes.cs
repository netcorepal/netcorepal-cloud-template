namespace ABC.Template.Domain;

/// <summary>
/// 错误码定义
/// </summary>
public sealed class ErrorCodes
{
    private ErrorCodes()
    {
        
    }

    #region 用户相关错误 (100xxx)

    /// <summary>
    /// 未找到账户
    /// </summary>
    public const int AccountNotFound = 100001;
    
    /// <summary>
    /// 用户名或密码错误
    /// </summary>
    public const int UserNameOrPasswordError = 100002;
    
    /// <summary>
    /// 未找到用户
    /// </summary>
    public const int UserNotFound = 100003;
    
    /// <summary>
    /// 无效的用户身份
    /// </summary>
    public const int InvalidUserIdentity = 100004;
    
    /// <summary>
    /// 无效的用户
    /// </summary>
    public const int InvalidUser = 100005;
    
    /// <summary>
    /// 无效的令牌
    /// </summary>
    public const int InvalidToken = 100006;
    
    /// <summary>
    /// 无效的刷新令牌
    /// </summary>
    public const int InvalidRefreshToken = 100007;
    
    #endregion
    
    #region 角色相关错误 (110xxx)
    
    /// <summary>
    /// 未找到角色
    /// </summary>
    public const int RoleNotFound = 110001;
    
    /// <summary>
    /// 不能删除管理员角色
    /// </summary>
    public const int CannotDeleteAdminRole = 110002;
    
    /// <summary>
    /// 角色已经被停用
    /// </summary>
    public const int RoleAlreadyDeactivated = 110003;
    
    /// <summary>
    /// 角色已经是激活状态
    /// </summary>
    public const int RoleAlreadyActivated = 110004;
    
    #endregion
    
    #region 部门相关错误 (120xxx)
    
    /// <summary>
    /// 未找到部门
    /// </summary>
    public const int DeptNotFound = 120001;
    
    /// <summary>
    /// 该部门下存在子部门，无法删除
    /// </summary>
    public const int DeptHasChildrenCannotDelete = 120002;
    
    /// <summary>
    /// 部门已经是激活状态
    /// </summary>
    public const int DeptAlreadyActivated = 120003;
    
    /// <summary>
    /// 部门已经被停用
    /// </summary>
    public const int DeptAlreadyDeactivated = 120004;
    
    /// <summary>
    /// 部门已经被删除
    /// </summary>
    public const int DeptAlreadyDeleted = 120005;
    
    /// <summary>
    /// 部门名称不能为空
    /// </summary>
    public const int DeptNameCannotBeEmpty = 120006;
    
    /// <summary>
    /// 子部门不能为空
    /// </summary>
    public const int ChildDeptCannotBeEmpty = 120007;
    
    #endregion
}
