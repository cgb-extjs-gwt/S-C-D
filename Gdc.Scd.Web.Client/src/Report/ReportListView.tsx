import { Container } from "@extjs/ext-react";
import * as React from "react";
import { buildComponentUrl } from "../Common/Services/Ajax";

export class ReportListView extends React.Component<any, any> {

    constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container padding="20px">
                <div onClick={this.onOpenLink}>
                    <a data-href="/report/abc">Sample report abc</a><br />
                    <a data-href="/report/xyz">Sample report xyz</a>
                </div>
            </Container>
        );
    }

    private init() {
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