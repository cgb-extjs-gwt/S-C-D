import { TableViewInfo } from "../../TableView/States/TableViewState";

export interface ITableViewService {
    getSchema(): Promise<TableViewInfo>;

    getUrl(): string;
}