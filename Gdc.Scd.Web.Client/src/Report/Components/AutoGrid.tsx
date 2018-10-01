import { Column, Container, Grid, NumberColumn, GridProps } from "@extjs/ext-react";
import * as React from "react";
import { AutoColumnModel } from "../Model/AutoColumnModel";
import { AutoColumnType } from "../Model/AutoColumnType";
import { AutoFilterModel } from "../Model/AutoFilterModel";
import { AutoFilter } from "./AutoFilter";

export interface AutoGridProps {

    url: string;

    columns: AutoColumnModel[];

    filter: AutoFilterModel[];

    title?: string;

}

export class AutoGrid extends React.Component<AutoGridProps, any> {

    private grid: Grid;

    private filter: AutoFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {

        proxy: {
            type: 'ajax',
            api: {
                read: this.props.url
            },
            reader: {
                type: 'json'
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

                <Grid
                    {...cfg}
                    ref="grid"
                    store={this.store}
                    width="100%">

                    {this.props.columns.map((v, i) => {

                        switch (v.type) {

                            case AutoColumnType.NUMBER:
                                return (
                                    <NumberColumn key={i} flex={v.flex || 1} text={v.text} dataIndex={v.name} />
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

    private showFilter() {
        let filter = this.props.filter;
        return filter && filter.length > 0;
    }
}