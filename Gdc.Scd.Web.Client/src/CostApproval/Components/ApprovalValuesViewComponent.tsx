import * as React from "react";
import { Grid, Column, Container, TextField, FormPanel, Toolbar, Button, Dialog } from "@extjs/ext-react";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { EditItem } from "../../CostEditor/States/CostBlockStates";

export interface CostBlockValueHistory {
    [key: string]: string | number, 
    HistoryValueId: number,
    Value: string 
}

export interface DetailsProps {
    columns?: ColumnInfo[]
    buildDataLoadUrl?(data: CostBlockValueHistory): string
}

export interface ApprovalValuesProps {
    dataLoadUrl?: string
    columns?: ColumnInfo[]
    id?: string
    details?: DetailsProps
    message?: string
}

interface ApprovalValuesState {
    isVisibleDetailWindow: boolean
    selectedRecords: { data: CostBlockValueHistory }[]
}

export class ApprovalValuesViewComponent extends React.Component<ApprovalValuesProps, ApprovalValuesState> {
    private readonly store;

    constructor(props: ApprovalValuesProps) {
        super(props);

        this.state = {
            isVisibleDetailWindow: false,
            selectedRecords: []
        }

        this.store = this.buildStore(props.columns, props.dataLoadUrl);
    }

    public render() {
        const { columns, id, message, children } = this.props;
        const { isVisibleDetailWindow } = this.state;

        return (
            <Container layout="vbox">
                {
                    message != null &&
                    <Container padding="10">
                        <span style={{fontWeight: "bold"}}>Message: </span>
                        {message}
                    </Container>
                }

                <DynamicGrid 
                    store={this.store} 
                    columns={columns} 
                    minHeight={400}
                    onSelectionChange={this.onSelectGrid}
                >
                    {
                        <Toolbar docked="top">
                            <Button 
                                text="Details" 
                                handler={this.onDetailButtonClick} 
                                width={100} 
                                disabled={this.state.selectedRecords.length != 1}
                            />
                        </Toolbar>
                    }
                </DynamicGrid>
                
                { children }
                {isVisibleDetailWindow && this.buildDetailWindow()}
            </Container>
        );
    }

    private buildStore(columns: ColumnInfo[], dataLoadUrl: string) {
        return Ext.create('Ext.data.Store', {
            fields: columns.map(column => ({ name: column.dataIndex })),
            autoLoad: true,
            proxy: {
                type: 'ajax',
                url: dataLoadUrl,
                reader: { 
                    type: 'json',
                }
            }
        });
    }

    private onSelectGrid = (grid, records: any[]) => {
        this.setState({
            selectedRecords: records
        })
    }

    private onDetailButtonClick = () => {
        this.setState({ isVisibleDetailWindow: true })
    }

    private buildDetailWindow() {
        const { isVisibleDetailWindow, selectedRecords } = this.state;
        const { columns, buildDataLoadUrl } = this.props.details;
        const dataLoadUrl = buildDataLoadUrl(selectedRecords[0].data);
        const store = this.buildStore(columns, dataLoadUrl);

        return (
            <Dialog 
                displayed={isVisibleDetailWindow} 
                title="Details" 
                closable 
                maximizable
                resizable={{
                    dynamic: true,
                    edges: 'all'
                }}
                layout="fit"
                onClose={this.closeDetailWindow}
            >
                <DynamicGrid store={store} columns={columns} minHeight={600}/>
            </Dialog>
        );
    }

    private closeDetailWindow = () => {
        this.setState({ 
            isVisibleDetailWindow: false 
        });
    }
}