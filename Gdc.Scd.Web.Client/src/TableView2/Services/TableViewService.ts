import { buildMvcUrl } from "../../Common/Services/Ajax";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { AppService } from "../../Layout/Services/AppService";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";
import { getTableViewInfo, updateRecords } from "../../TableView/Services/TableViewService";
import { TableViewRecord } from "../../TableView/States/TableViewRecord";
import { QualityGateResultSet, TableViewInfo } from "../../TableView/States/TableViewState";
import { ITableViewService } from "./ITableViewService";

export class TableViewService implements ITableViewService {

    private static schema: any; // schema promise cache

    private controllerName: string = 'TableView';

    public getMeta(): Promise<CostMetaData> {
        return new AppService().getCostMetaData();
    }

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

    public updateRecords(records: TableViewRecord[], approvalOption: ApprovalOption): Promise<QualityGateResultSet> {
        return updateRecords(records, approvalOption);
    }
}