import { Button, Container, Grid, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { AlertHelper } from "../../Common/Helpers/AlertHelper";
import { AutoColumnModel } from "../Model/AutoColumnModel";
import { AutoFilterModel } from "../Model/AutoFilterModel";
import { AutoColumnBuilder } from "./AutoColumnBuilder";
import { AutoFilter } from "./AutoFilter";

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

    private columns: AutoColumnBuilder[];

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

        let title = null;

        if (this.props.title) {
            title = <p>[&nbsp;{this.props.title}&nbsp;]</p>;
        }

        return (
            <Container layout="fit">

                <AutoFilter ref="filter" docked="right" hidden={!this.showFilter()} filter={this.props.filter} onSearch={this.onSearch} scrollable={true} />

                <Toolbar docked="top">
                    {title}
                    <Button iconCls="x-fa fa-download" text="Download" handler={this.onDownload} />
                </Toolbar>

                <Grid
                    ref="grid"
                    store={this.store}
                    width="100%"
                    defaults={{ minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}
                    plugins={['pagingtoolbar']}>

                    {this.columns.map((x, i) => x.build(i))}

                </Grid>

            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as AutoFilter;
    }

    private init() {
        this.columns = this.props.columns.map(x => new AutoColumnBuilder(x));
        //
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);
        this.onDownload = this.onDownload.bind(this);

        this.store.on('beforeload', this.onBeforeLoad);
    }

    private onDownload() {
        let filter = this.filter.getModel() || {};
        AlertHelper.autoload(this.props.downloadUrl, '', filter);
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