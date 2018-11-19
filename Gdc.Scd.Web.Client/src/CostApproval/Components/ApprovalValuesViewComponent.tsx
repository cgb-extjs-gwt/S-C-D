import * as React from "react";
import { Grid, Column, Container, TextField, FormPanel, Toolbar, Button, Dialog } from "@extjs/ext-react";
import { EditItem } from "../../CostEditor/States/CostBlockStates";
import { QualityGateGrid } from "../../QualityGate/Components/QualityGateGrid";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";

export interface DetailsProps {
    buildDataLoadUrl?(data: BundleDetailGroup): string
    inputLevelId: string
}

export interface ApprovalValuesProps {
    dataLoadUrl?: string
    costElement: CostElementMeta
    inputLevelId: string
    id?: string
    details?: DetailsProps
    message?: string
    hideCheckColumns?: boolean
}

interface ApprovalValuesState {
    isVisibleDetailWindow: boolean
    selectedRecords: { data: BundleDetailGroup }[]
}

export class ApprovalValuesViewComponent extends React.Component<ApprovalValuesProps, ApprovalValuesState> {
    private readonly storeConfig

    constructor(props: ApprovalValuesProps) {
        super(props);

        this.state = {
            isVisibleDetailWindow: false,
            selectedRecords: []
        }

        this.storeConfig = this.buildStoreConfig(props.dataLoadUrl);
    }

    public render() {
        const { id, message, children, costElement, hideCheckColumns, inputLevelId } = this.props;
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

                <QualityGateGrid 
                    costElement={costElement} 
                    storeConfig={this.storeConfig} 
                    hideCheckColumns={hideCheckColumns} 
                    inputLevelId={inputLevelId}
                    minHeight={400}
                    onSelectionChange={this.onSelectGrid}
                >
                    <Toolbar docked="top">
                        <Button 
                            text="Details" 
                            handler={this.onDetailButtonClick} 
                            width={100} 
                            disabled={this.state.selectedRecords.length != 1}
                        />
                    </Toolbar>
                </QualityGateGrid>

                { children }
                {isVisibleDetailWindow && this.buildDetailWindow()}
            </Container>
        );
    }

    private buildStoreConfig(dataLoadUrl: string) {
        return {
            autoLoad: true,
            proxy: {
                type: 'ajax',
                url: dataLoadUrl,
                reader: { 
                    type: 'json',
                }
            }
        };
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
        const { costElement, hideCheckColumns } = this.props;
        const { buildDataLoadUrl, inputLevelId } = this.props.details;
        const dataLoadUrl = buildDataLoadUrl(selectedRecords[0].data);
        const storeConfig = this.buildStoreConfig(dataLoadUrl);

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
                <QualityGateGrid 
                    costElement={costElement} 
                    storeConfig={storeConfig} 
                    hideCheckColumns={hideCheckColumns} 
                    inputLevelId={inputLevelId} 
                    minHeight={600}
                />
            </Dialog>
        );
    }

    private closeDetailWindow = () => {
        this.setState({ 
            isVisibleDetailWindow: false 
        });
    }
}