import { Container } from "@extjs/ext-react";
import * as React from "react";
import { buildComponentUrl } from "../Common/Services/Ajax";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";
import { ReportModel } from "./Model/ReportModel";

export interface ReportListViewState {
    list: ReportModel[];
};

export class ReportListView extends React.Component<any, ReportListViewState> {

    private srv: IReportService;

    public state: ReportListViewState = {
        list: null
    };

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public componentDidMount() {
        this.srv.getReports().then(x => this.setState({ list: x.items }));
    }

    public render() {
        let reports = this.state.list || [];
        return (
            <Container padding="20px">
                <div onClick={this.onOpenLink}>

                    {reports.map((x, i) => {
                        return (
                            <div key={i}>
                                <a data-href={'/report/' + x.id}>{x.name}</a><br /><br />
                            </div>
                        );
                    })}

                </div>
            </Container>
        );
    }

    private init() {
        this.srv = ReportFactory.getReportService();
        this.onOpenLink = this.onOpenLink.bind(this);
    }

    private onOpenLink(e) {

        let target = e.target as HTMLElement;
        let href = target.getAttribute('data-href');

        if (href) {
            href = buildComponentUrl(href);
            this.props.history.push(href);
        }
    }

}