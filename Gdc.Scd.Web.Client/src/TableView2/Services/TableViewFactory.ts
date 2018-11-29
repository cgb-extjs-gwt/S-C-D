import { ITableViewService } from "./ITableViewService";
import { TableViewService } from "./TableViewService";

export class TableViewFactory {
    public static getTableViewService(): ITableViewService {
        return new TableViewService();
    }
}