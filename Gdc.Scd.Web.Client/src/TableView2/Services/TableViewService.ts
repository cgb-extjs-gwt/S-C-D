import { getTableViewInfo } from "../../TableView/Services/TableViewService";
import { ITableViewService } from "./ITableViewService";
import { TableViewInfo } from "../../TableView/States/TableViewState";
import { buildMvcUrl } from "../../Common/Services/Ajax";

export class TableViewService implements ITableViewService {

    private static schema: any; // schema promise cache

    private controllerName: string = 'TableView';

    public getSchema(): Promise<TableViewInfo> {
        let p = TableViewService.schema;

        if (!p) {
            p = getTableViewInfo();
            TableViewService.schema = p;
        }

        return p;
    }

    public getUrl(): string {
        return buildMvcUrl(this.controllerName, 'GetRecords');
    }

}