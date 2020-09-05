const rules = {
    Anonymous: {
        static: ["vacations:list", "home-page:visit"]
    },
    Employee: {
        static: [
            "vacations:list",
            "users:getSelf",
            "home-page:visit",
        ],
        dynamic: {
            "vacations:create": ({ userId, vacationOwnerId }) => {
                if (!userId || !vacationOwnerId) return false;
                return userId === vacationOwnerId;
            },
            "vacations:edit": ({ userId, vacationOwnerId }) => {
                if (!userId || !vacationOwnerId) return false;
                return userId === vacationOwnerId;
            },
            "vacations:delete": ({ userId, vacationOwnerId }) => {
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