import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { AutoGrid } from "./Components/AutoGrid";
import { AutoGridModel } from "./Model/AutogridModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";

export class ReportView extends React.Component<any, AutoGridModel> {

    private type: string;

    private srv: IReportService;

    constructor(props: any) {

        var type = props.match.params.type;

        if (!type) {
            throw new Error('invalid report type');
        }

        super(props);
        this.init();

        this.type = type;
    }

    public componentDidMount() {
        this.srv.getSchema(this.type).then(x => this.setState(x));
    }

    public render() {

        let grid = null;

        if (this.state) {
            let schema = this.state;

            grid = (
                <AutoGrid columns={schema.fields} filter={schema.filter} url={this.getUrl()} title={schema.caption} />
            );
        }

        return grid;
    }

    private init() {
        this.srv = ReportFactory.getReportService();
    }

    public getUrl(): string {
        return buildMvcUrl('report', 'view', { 'type': this.type });
    }
}