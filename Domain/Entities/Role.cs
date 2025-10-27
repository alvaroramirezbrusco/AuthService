namespace Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public IList<User> Users { get; set; }
    }
}
