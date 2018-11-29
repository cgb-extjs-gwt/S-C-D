import { TableViewInfo } from "../../TableView/States/TableViewState";
import { CostMetaData } from "../../Common/States/CostMetaStates";

export interface ITableViewService {
    getMeta(): Promise<CostMetaData>;

    getSchema(): Promise<TableViewInfo>;

    getUrl(): string;
}