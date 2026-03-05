namespace MooreHotelAndSuites.Domain.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Receptionist = "Receptionist";
        public const string HouseKeeping = "HouseKeeping";
        public const string Kitchen = "Kitchen";
        public const string Bar = "Bar";
        public const string Laundry = "Laundry";
        public const string User = "User";  // For registered guests

        public static readonly string[] StaffRoles =
        {
            Admin,
            Manager,
            Receptionist,
            HouseKeeping,
            Kitchen,
            Bar,
            Laundry
        };

        public static readonly string[] AllRoles =
        {
            Admin,
            Manager,
            Receptionist,
            HouseKeeping,
            Kitchen,
            Bar,
            Laundry,
            User  
        };
    }
}