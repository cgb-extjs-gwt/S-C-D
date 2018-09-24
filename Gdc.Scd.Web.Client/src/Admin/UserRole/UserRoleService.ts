import { NamedId, DataInfo } from "../../Common/States/CommonStates";
import { UserRoleFilterModel } from "./UserRoleFilterModel";

import { UserService } from "../../Dict/Services/UserService";
import { RoleService } from "../../Dict/Services/RoleService";
import { CountryService } from "../../Dict/Services/CountryService";

import { get, post } from "../../Common/Services/Ajax";

export class UserRoleService {

    private controllerName: string;

    public constructor() {
        this.controllerName = 'UserRole';
    }

    public getUsers(): Promise<NamedId<string>[]> {
        return new UserService().getAll();
    }
    public getRoles(): Promise<NamedId<string>[]> {
        return new RoleService().getAll();
    }

    public getCountries(): Promise<NamedId<string>[]> {
        return new CountryService().getAll();
    }

    public getUserRoles(filter: UserRoleFilterModel): Promise<DataInfo<UserRoleFilterModel>> {
        return get<DataInfo<UserRoleFilterModel>>(this.controllerName, 'getBy', filter);
    }

}
