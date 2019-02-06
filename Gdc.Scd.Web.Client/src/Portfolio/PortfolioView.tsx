import { Button, Container, Grid, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { buildComponentUrl, buildMvcUrl, post } from "../Common/Services/Ajax";
import { FilterPanel } from "./Components/FilterPanel";
import { NullStringColumn } from "./Components/NullStringColumn";
import { ReadonlyCheckColumn } from "./Components/ReadonlyCheckColumn";
import { PortfolioFilterModel } from "./Model/PortfolioFilterModel";
import { IPortfolioService } from "./Services/IPortfolioService";
import { PortfolioServiceFactory } from "./Services/PortfolioServiceFactory";
import { UserCountryService } from "../Dict/Services/UserCountryService";

export class PortfolioView extends React.Component<any, any> {

    private grid: Grid;

    private filter: FilterPanel;

    private srv: IPortfolioService;

    state = {
        isCountryUser: true
    };

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 100,
        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('portfolio', 'allowed')
            },
            reader: {
                type: 'json',
                keepRawData: true,
                rootProperty: 'items',
                totalProperty: 'total'             
            }
        }
    });

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let isLocalPortfolio = this.isLocalPortfolio();
        let isCountryUser = this.state.isCountryUser;

        return (
            <Container scrollable={true}>

                <FilterPanel ref="filter" docked="right" onSearch={this.onSearch} scrollable={true} isCountryUser={isCountryUser}/>

                <Toolbar docked="top">
                    <Button iconCls="x-fa fa-edit" text="Edit" handler={this.onEdit} />
                    <Button iconCls="x-fa fa-undo" text="Deny combinations" ui="decline" disabled={!isLocalPortfolio} handler={this.onDeny} />
                </Toolbar>

                <Grid
                    ref={x => this.grid = x}
                    store={this.store}
                    width="100%"
                    height="100%"
                    selectable="multi"
                    plugins={['pagingtoolbar']}>

                    <NullStringColumn hidden={!isLocalPortfolio && !isCountryUser} flex="1" text="Country" dataIndex="country" />

                    <NullStringColumn flex="1" text="WG(Asset)" dataIndex="wg" />
                    <NullStringColumn flex="1" text="Availability" dataIndex="availability" />
                    <NullStringColumn flex="1" text="Duration" dataIndex="duration" />
                    <NullStringColumn flex="1" text="Reaction type" dataIndex="reactionType" />
                    <NullStringColumn flex="1" text="Reaction time" dataIndex="reactionTime" />
                    <NullStringColumn flex="2" text="Service location" dataIndex="serviceLocation" />
                    <NullStringColumn flex="2" text="ProActive" dataIndex="proActive" />

                    <ReadonlyCheckColumn hidden={isLocalPortfolio || isCountryUser} flex="1" text="Fujitsu principal portfolio" dataIndex="isGlobalPortfolio" />
                    <ReadonlyCheckColumn hidden={isLocalPortfolio || isCountryUser} flex="1" text="Master portfolio" dataIndex="isMasterPortfolio" />
                    <ReadonlyCheckColumn hidden={isLocalPortfolio || isCountryUser} flex="1" text="Core portfolio" dataIndex="isCorePortfolio" />
                </Grid>

            </Container>
        );
    }

    public componentDidMount() {
        this.filter = this.refs.filter as FilterPanel;
        this.reload();
    }

    private init() {
        this.srv = PortfolioServiceFactory.getPortfolioService();
        this.onEdit = this.onEdit.bind(this);
        this.onDeny = this.onDeny.bind(this);
        this.onSearch = this.onSearch.bind(this);
        this.store.on('beforeload', this.onBeforeLoad, this);

        const srv = new UserCountryService();
        srv.isCountryUser().then(x => this.setState({ isCountryUser: x }));
    }

    private onEdit() {
        this.props.history.push(buildComponentUrl('/portfolio/edit'));
    }

    private onDeny() {
        let selected = this.getSelectedRows();
        if (selected.length > 0 && this.isLocalPortfolio()) {
            ExtMsgHelper.confirm('Deny combinations', 'Do you want to remove combination(s)?', () => this.denyCombination(selected));
        }
    }

    private onSearch(filter: PortfolioFilterModel) {
        this.reload();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }

    private denyCombination(ids: string[]) {
        let cnt = this.getSelectedCountry();
        let p = this.srv.denyById(cnt[0], ids).then(x => this.reload());
        handleRequest(p);
    }

    private reload() {
        this.store.load();

        this.setState({ ___: new Date().getTime() }); //stub, re-paint ext grid
    }

    private isLocalPortfolio(): boolean {
        let result = false;
        if (this.filter) {
            let filter = this.filter.getModel();
            result = !!(filter && filter.country && filter.country.length > 0);
        }
        return result;
    }

    private getSelectedCountry() {
        return this.filter.getModel().country;
    }

    private getSelectedRows(): string[] {
        return ExtDataviewHelper.getGridSelected(this.grid, 'id');
    }
}