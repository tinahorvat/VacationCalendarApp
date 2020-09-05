const rules = {
    Anonymous: {
        static: ["vacations:list", "home-page:visit"]
    },
    Employee: {
        static: [
            "vacations:list",
            "vacations:create",
            "users:getSelf",
            "home-page:visit",
        ],
        dynamic: {
            "vacations:edit": ({ userId, vacationOwnerId }) => {
                if (!userId || !vacationOwnerId) return false;
                return userId === vacationOwnerId;
            }
        }
    },
    Admin: {
        static: [
            "vacations:list",
            "vacations:create",
            "vacations:edit",
            "vacations:delete",
            "users:get",
            "users:getSelf",
            "home-page:visit",           
        ]
    }
};

export default rules;