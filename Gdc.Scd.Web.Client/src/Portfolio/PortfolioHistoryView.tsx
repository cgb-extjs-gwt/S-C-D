import { Button, Column, Container, DateColumn, Grid, GridCell, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { DateFormats } from "../Common/Helpers/DateHelpers";
import { buildComponentUrl, buildMvcUrl } from "../Common/Services/Ajax";
import { IRenderer } from "../Report/Components/GridRenderer";
import { ReadonlyCheckColumn } from "./Components/ReadonlyCheckColumn";

export class PortfolioHistoryView extends React.Component<any, any> {

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        fields: [
            { name: 'editDate', type: 'date' },
            { name: 'editUser', type: 'string' },
        ],

        pageSize: 50,
        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('portfolio', 'history')
            },
            actionMethods: {
                read: 'POST'
            },
            reader: {
                type: 'json',
                keepRawData: true,
                rootProperty: 'items',
                totalProperty: 'total'
            },
            paramsAsJson: true
        }
    });

    private wgRndr = this.joinRenderer('wgs');
    private avRndr = this.joinRenderer('availabilities');
    private durRndr = this.joinRenderer('durations');
    private rtypeRndr = this.joinRenderer('reactionTypes');
    private rtimeRndr = this.joinRenderer('reactionTimes');
    private locRndr = this.joinRenderer('serviceLocations');
    private proRndr = this.joinRenderer('proActives');

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public componentDidMount() {
        this.reload();
    }

    public render() {
        return <Container layout="vbox" padding="10px" scrollable="true">

            <Toolbar docked="top">
                <Button iconCls="x-fa fa-arrow-left" text="back to Portfolio" handler={this.onBack} />
            </Toolbar>

            <Grid
                store={this.store}
                width="100%"
                height="100%"
                plugins={['pagingtoolbar']}>

                <DateColumn flex="1" text="Date" dataIndex="editDate" format={DateFormats.dateTime} />
                <Column flex="1" text="User" dataIndex="editUser" />
                <Column flex="1" text="Country/Principal" dataIndex="country" renderer={this.countryRndr} >
                    <GridCell encodeHtml={false} />
                </Column>
                <ReadonlyCheckColumn flex="1" text="Deny" dataIndex="deny" />
                <Column flex="1" text="WG(Asset)" dataIndex="rules" renderer={this.wgRndr}>
                    <GridCell encodeHtml={false} />
                </Column>
                <Column flex="1" text="Availability" dataIndex="rules" renderer={this.avRndr}>
                    <GridCell encodeHtml={false} />
                </Column>
                <Column flex="1" text="Duration" dataIndex="rules" renderer={this.durRndr}>
                    <GridCell encodeHtml={false} />
                </Column>
                <Column flex="1" text="Reaction type" dataIndex="rules" renderer={this.rtypeRndr}>
                    <GridCell encodeHtml={false} />
                </Column>
                <Column flex="1" text="Reaction time" dataIndex="rules" renderer={this.rtimeRndr}>
                    <GridCell encodeHtml={false} />
                </Column>
                <Column flex="2" text="Service location" dataIndex="rules" renderer={this.locRndr}>
                    <GridCell encodeHtml={false} />
                </Column>
                <Column flex="2" text="ProActive" dataIndex="rules" renderer={this.proRndr}>
                    <GridCell encodeHtml={false} />
                </Column>

            </Grid>

        </Container>
    }

    private countryRndr(value: any, row: any): string {
        let d = row.data;
        if (!d) {
            return ' ';
        }

        if (d.country) {
            return d.country;
        }

        d = d.rules;

       let arr = [];
        if (d.isGlobalPortfolio) {
            arr.push('Fujitsu principal');
        }

        if (d.isMasterPortfolio) {
            arr.push('Master');
        }

        if (d.isGlobalPortfolio) {
            arr.push('Core');
        }

        return arr.join(',<br>');
    }

    private joinRenderer(field: string): IRenderer {
        return function (value: any, row: any): string {
            let d = row.data.rules;
            if (d && d[field]) {
                d = d[field];
                if (d.length > 0) {
                    return d.join(',<br/>');
                }
            }
            return ' ';
        }
    }

    private init() {
        this.onBack = this.onBack.bind(this);
    }

    private onBack() {
        this.openLink('/portfolio');
    }

    private openLink(url: string) {
        this.props.history.push(buildComponentUrl(url));
    }

    private reload() {
        this.store.load();
    }
}