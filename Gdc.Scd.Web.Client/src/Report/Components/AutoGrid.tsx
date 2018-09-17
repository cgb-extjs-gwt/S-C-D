import { Column, Container, Grid } from "@extjs/ext-react";
import * as React from "react";
import { AutoFilter } from "./AutoFilter";

export abstract class AutoGrid extends React.Component<any, any> {

    private grid: Grid;

    private filter: AutoFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {

        pageSize: 25,

        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: this.getUrl()
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            }
        }
    });

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public abstract getColumns(): Column[];

    public abstract getFilter(): React.Component[];

    public abstract getUrl(): string;

    public render() {
        return (
            <Container layout="fit">

                <AutoFilter ref="filter" docked="right" filter={this.getFilter} onSearch={this.onSearch} />

                <Grid
                    ref="grid"
                    store={this.store}
                    width="100%"
                    plugins={['pagingtoolbar']}>

                    {this.getColumns()}

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

        this.store.on('beforeload', this.onBeforeLoad);
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
}