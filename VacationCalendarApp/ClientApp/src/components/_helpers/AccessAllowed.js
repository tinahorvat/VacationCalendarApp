﻿import rules from "./RoleAccessRules";

const check = (rules, role, action, data) => {
    const permissions = rules[role];
    if (!permissions) {
        // role is not present in the rules
        return false;
    }

    const staticPermissions = permissions.static;

    if (staticPermissions && staticPermissions.includes(action)) {
        // static rule not provided for action
        return true;
    }

    const dynamicPermissions = permissions.dynamic;

    if (dynamicPermissions) {
        const permissionCondition = dynamicPermissions[action];
        if (!permissionCondition) {
            // dynamic rule not provided for action
            return false;
        }

        return permissionCondition(data);
    }
    return false;
};

const AccessAlowed = props =>
    check(rules, props.role, props.perform, props.data)
        ? props.yes()
        : props.no();

AccessAlowed.defaultProps = {
    yes: () => null,
    no: () => null
};

export default AccessAlowed;