import * as React from 'react';
import { Grid, Column, CheckColumn, Toolbar, Button, Container } from '@extjs/ext-react';
import { buildMvcUrl } from "../../Common/Services/Ajax";
import { FilterPanel } from "./AvailabilityFeeFilterPanel";
import { AvailabilityFeeFilterModel } from "./AvailabilityFeeFilterModel";

const CONTROLLER_NAME = 'AvailabilityFeeAdmin';

class AvailabilityFeeAdminGrid extends React.Component{

    private filter: FilterPanel;

    state = {
        disableSaveButton: true
    };

    store = Ext.create('Ext.data.Store', {
        pageSize: 50,
        fields: ['countryName', 'countryId', 'reactionTimeName', 
                 'reactionTimeId', 'reactionTypeName', 'reactionTypeId',
                'serviceLocatorName', 'serviceLocatorId', 'isApplicable', 'innerId'],
        autoLoad: true,
        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl(CONTROLLER_NAME, 'GetAll'),
                update: buildMvcUrl(CONTROLLER_NAME, 'SaveAll')
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            },
            writer: {
                type: 'json',
                writeAllFields: true,
                allowSingle: false
            },
            listeners: {
                exception: function(proxy, response, operation){
                    //TODO: show error
                }
            }
        },
        listeners:{
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const modifiedRecords = this.store.getUpdatedRecords();
                if (modifiedRecords.length > 0){
                    this.setState({ disableSaveButton: false});
                }
                else{
                    this.setState({ disableSaveButton: true});
                }
            }
        }
    });

    public constructor(props: any) {
        super(props);
        this.init();
    }

    saveRecords = () => {

        this.store.sync({
            callback: function(batch, options){
            },

            success: function(batch, options){
                //TODO: show successfull message box
            },

            failure: (batch, options) =>
            {
                //TODO: show error
            }
            
        });
    }

    render(){
        return (
            <Container scrollable={true} >

                <FilterPanel ref="filter" docked="right" onSearch={this.onSearch} scrollable={true} />

                <Grid
                    title={'Availability Fee Settings'}
                    store={this.store}
                    cls="filter-grid myGrid"
                    columnLines={true}
                    width="100%"
                    height="100%"
                    plugins={['pagingtoolbar']} >
                    <Column text="Country" dataIndex="countryName" flex={1} />
                    <Column text="Reaction Time" dataIndex="reactionTimeName" flex={1} />
                    <Column text="Reaction Type" dataIndex="reactionTypeName" flex={1} />
                    <Column text="Service Location" dataIndex="serviceLocatorName" flex={1} />
                    <CheckColumn text="Is Applicable" dataIndex="isApplicable" flex={1} />

                    <Toolbar docked="bottom">
                        <Button
                            text="Save"
                            flex={1}
                            handler={this.saveRecords}
                            iconCls="x-fa fa-save"
                            disabled={this.state.disableSaveButton}
                        />
                    </Toolbar>
                </Grid>
            </Container>
           );
    }

    public componentDidMount() {
        this.filter = this.refs.filter as FilterPanel;
        //
        this.reload();
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);
        //
        this.store.on('beforeload', this.onBeforeLoad);
    }

    private onSearch(filter: AvailabilityFeeFilterModel) {
        this.reload();
    }

    private reload() {
        this.store.currentPage = 1
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params); 
    }
}

export default AvailabilityFeeAdminGrid;