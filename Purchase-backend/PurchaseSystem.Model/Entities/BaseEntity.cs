namespace PurchaseSystem.Model.Entities
{
    /// <summary>
    /// 基础实体
    /// </summary>
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
