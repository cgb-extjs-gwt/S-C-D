import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { AutoGrid } from "./Components/AutoGrid";
import { AutoColumnModel } from "./Model/AutoColumnModel";
import { AutoColumnType } from "./Model/AutoColumnType";
import { AutoFilterModel } from "./Model/AutoFilterModel";

export class ReportView extends React.Component<any, any> {

    constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <AutoGrid columns={this.getColumns()} filter={this.getFilter()} url={this.getUrl()} />
        );
    }

    private init() {
    }

    public getColumns(): AutoColumnModel[] {
        return [
            { name: 'col_1', text: 'Super fields 1', type: AutoColumnType.NUMBER },
            { name: 'col_2', text: 'Super fields 2', type: AutoColumnType.TEXT },
            { name: 'col_3', text: 'Super fields 3', type: AutoColumnType.TEXT },
            { name: 'col_4', text: 'Super fields 4', type: AutoColumnType.TEXT }
        ];
    }

    public getFilter(): AutoFilterModel[] {
        return [
            { name: 'col_1', text: 'Super fields 1', type: AutoColumnType.NUMBER },
            { name: 'col_2', text: 'Super fields 2', type: AutoColumnType.TEXT },
            { name: 'col_4', text: 'Super fields 4', type: AutoColumnType.TEXT }
        ];
    }

    public getUrl(): string {
        return buildMvcUrl('report', 'GetReport', { 'type': 'tst' });
    }
}