import { CostMetaData } from "../../Common/States/CostMetaStates";
import { get } from "../../Common/Services/Ajax";
import { AppData, Role } from "../States/AppStates";

export function getAppData(): Promise<AppData> {
    return new AppService().getAppData();
}

export class AppService {

    private static schema: any; // schema promise cache

    private controllerName: string = 'App';

    public getAppData(): Promise<AppData> {
        let p = AppService.schema;

        if (!p) {
            p = get<AppData>(this.controllerName, 'GetAppData');
            AppService.schema = p;
        }

        return p;
    }

    public getCostMetaData(): Promise<CostMetaData> {
        return this.getAppData().then(x => x.meta);
    }

    public getRoles(): Promise<Role[]> {
        return this.getAppData().then(x => x.userRoles);
    }

    public hasGlobalRole(): Promise<boolean> {
        return this.getRoles().then(x => x.some(y => y.isGlobal));
    }

}