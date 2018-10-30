import { Button, Column, Container, Grid, GridProps, NumberColumn, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { getFromUri } from "../../Common/Services/Ajax";
import { AutoColumnModel } from "../Model/AutoColumnModel";
import { AutoColumnType } from "../Model/AutoColumnType";
import { AutoFilterModel } from "../Model/AutoFilterModel";
import { AutoFilter } from "./AutoFilter";
import { EuroStringColumn } from "./EuroStringColumn";
import { PercentColumn } from "./PercentColumn";

export interface AutoGridProps {

    url: string;

    downloadUrl: string;

    columns: AutoColumnModel[];

    filter: AutoFilterModel[];

    title?: string;

}

export class AutoGrid extends React.Component<AutoGridProps, any> {

    private grid: Grid;

    private filter: AutoFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 25,
        proxy: {
            type: 'ajax',
            api: {
                read: this.props.url
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            }
        }
    });

    public constructor(props: AutoGridProps) {
        super(props);
        this.init();
    }

    public render() {

        let cfg = {} as GridProps;

        if (this.props.title) {
            cfg.title = this.props.title;
        }

        return (
            <Container layout="fit">

                <AutoFilter ref="filter" docked="right" hidden={!this.showFilter()} filter={this.props.filter} onSearch={this.onSearch} />

                <Toolbar docked="top">
                    <Button iconCls="x-fa fa-download" text="Download" handler={this.onDownload} />
                </Toolbar>

                <Grid
                    {...cfg}
                    ref="grid"
                    store={this.store}
                    width="100%"
                    defaults={{ minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}
                    plugins={['pagingtoolbar']}>

                    {this.props.columns.map((v, i) => {

                        switch (v.type) {

                            case AutoColumnType.NUMBER:
                                return (
                                    <NumberColumn key={i} flex={v.flex || 1} text={v.text} dataIndex={v.name} />
                                );

                            case AutoColumnType.EURO:
                                return (
                                    <EuroStringColumn key={i} flex={v.flex || 1} text={v.text} dataIndex={v.name} />
                                );

                            case AutoColumnType.PERCENT:
                                return (
                                    <PercentColumn key={i} flex={v.flex || 1} text={v.text} dataIndex={v.name} />
                                );

                            case AutoColumnType.TEXT:
                            default:
                                return (
                                    <Column key={i} flex={v.flex || 1} text={v.text} dataIndex={v.name} />
                                );

                        }
                    })}


                </Grid>

            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as AutoFilter;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);
        this.onDownload = this.onDownload.bind(this);

        this.store.on('beforeload', this.onBeforeLoad);
    }

    private onDownload() {
        let url = this.props.downloadUrl;

        let filter = this.filter.getModel() || {};
        filter._dc = new Date().getTime();

        url = Ext.urlAppend(url, Ext.urlEncode(filter, true));

        let p = getFromUri<any>(url);
        handleRequest(p).then(x => Ext.Msg.alert('Report', 'Your report in process...<br>Please wait while it finish'));
    }

    private onSearch(filter: any) {
        this.reload();
    }

    private reload() {
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }

    private showFilter() {
        let filter = this.props.filter;
        return filter && filter.length > 0;
    }
}