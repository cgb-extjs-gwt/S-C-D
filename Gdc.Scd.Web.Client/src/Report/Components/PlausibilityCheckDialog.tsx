import * as React from "react";
import { Dialog, Container, Grid, Column, TextColumn, GridCell } from "@extjs/ext-react";
import { emptyRenderer } from "./GridRenderer";
import { getFromUri } from "../../Common/Services/Ajax";
import { handleRequest } from "../../Common/Helpers/RequestHelper";

export interface PlausibilityCheckProps {

}

function mandatoryRenderer(val, row) {
    return val ? '<span class="red">*</span>' : ' ';
}

export class PlausibilityCheckDialog extends React.Component<PlausibilityCheckProps, any> {

    private grid: Grid & any;

    public state: any = {
        onlyMissing: false
    };

    public componentDidMount() {
        let p = getFromUri('http://localhost:11167/scd/Content/fake/cost-block.json').then(x => this.setState({ data: x }));
        handleRequest(p);
    }

    public render() {

        let d = this.state.data;

        if (!d) {
            return null;
        }

        return <Dialog
            displayed={true}
            closable
            closeAction="hide"
            draggable={false}
            minWidth="650px"
            maxWidth="1400px"
            width="75%"
            height="90%"
            title="Plausibility check"
        >


            <div className="plausi-box">
                <div className="plausi-box-left">
                    <h1>{d.name}</h1>
                    <h3>
                        FSP:GA3S60Z00MES8B: Germany, ACD, 9x5, 4 years, no Reaction, Bring-In, 3 years STDW, no Proactive SLA
                    </h3>
                </div>
                <h3 className="plausi-box-right">1500 RUB(500 EUR)</h3>
            </div>

            <a className="lnk underline" onClick={this.onShowMissing}>
                {this.state.onlyMissing ? 'Show all elements' : 'Show missing elements only'}
            </a>
            <br />

            <table className="plausi-tbl">
                <tr>
                    <th></th>
                    <th>Cost element</th>
                    <th>Value</th>
                    <th>Dependency</th>
                    <th>Level</th>
                </tr>
            </table>

        </Dialog>;

            //<Grid
            //    ref={x => this.grid = x}
            //    store={this.store}
            //    width="100%"
            //    height="100%"
            //    columnLines={true}
            //>

            //    <Column text="&nbsp" dataIndex="mandatory" renderer={mandatoryRenderer}>
            //        <GridCell encodeHtml={false} />
            //    </Column>
            //    <Column flex="1" text="Cost element" dataIndex="name" />
            //    <Column flex="1" text="Value" dataIndex="value" renderer={emptyRenderer} />
            //    <Column flex="1" text="Dependency" dataIndex="dependency" renderer={emptyRenderer} />
            //    <Column flex="1" text="Level" dataIndex="level" />

            //</Grid>
    }

    private onShowMissing = () => {
        let missing = !this.state.onlyMissing;
        this.setState({ onlyMissing: missing });
        //
        //this.store.clearFilter();
        //if (missing) {
        //    this.store.filterBy(x => x.data.value === null || x.data.value === undefined);
        //}
    }

}