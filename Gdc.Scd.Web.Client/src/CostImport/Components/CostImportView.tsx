import * as React from "react";
import { NamedId, SelectListAdvanced } from "../../Common/States/CommonStates";
import { Container, Panel, FormPanel, Grid, ComboBoxField, Toolbar, Button, FileField, Column } from "@extjs/ext-react";
import { Store, Model } from "../../Common/States/ExtStates";
import { QualityGateWindowContainer, QualityGateWindowContainerProps } from "./QualityGateWindowContainer"

export interface ResultImportItem {
    info: string
}

export interface CostImportViewProps {
    applications: SelectListAdvanced<NamedId> 
    costBlocks: SelectListAdvanced<NamedId>
    costElements: SelectListAdvanced<NamedId>
    dependencyItems?: SelectListAdvanced<NamedId<number>, number>
    regions?: SelectListAdvanced<NamedId<number>, number>
    isVisibleDependencyItems: boolean
    isVisibleRegions: boolean
    resultImport: ResultImportItem[]
    isImportButtonEnabled: boolean
    onImport?(file)
    onFileSelect?(fileName: string)
    onUnmount?()
}

interface SelectListSelector {
    (props: CostImportViewProps): SelectListAdvanced<NamedId<any>, any>
}

interface ComboboxData { 
    onChange(combobox, newValue, oldValue)
    selector: SelectListSelector
    buildConfig(): {
        store: Store<NamedId>
        selection: Model<NamedId>
        displayField: string
        valueField: string
        onChange(combobox, newValue, oldValue)
    }
}

export class CostImportView extends React.PureComponent<CostImportViewProps> {
    private applicationData: ComboboxData
    private costBlockData: ComboboxData
    private costElementData: ComboboxData
    private dependencyData: ComboboxData
    private regionData: ComboboxData
    private resultStore: Store<ResultImportItem>
    private fileField

    constructor(props: CostImportViewProps) {
        super(props)

        this.applicationData = this.buildComboboxData(props => props.applications);
        this.costBlockData = this.buildComboboxData(props => props.costBlocks);
        this.costElementData = this.buildComboboxData(props => props.costElements);
        this.dependencyData = this.buildComboboxData(props => props.dependencyItems);
        this.regionData = this.buildComboboxData(props => props.regions)
        this.resultStore = this.createStore(props.resultImport)
    }

    public componentWillReceiveProps(nextProps: CostImportViewProps) {
        this.updateStore(this.props.resultImport, nextProps.resultImport, this.resultStore);
    }

    public componentWillUnmount() {
        const { onUnmount } = this.props;

        onUnmount && onUnmount();
    }

    public render() {
        const { dependencyItems, isImportButtonEnabled, isVisibleDependencyItems, isVisibleRegions } = this.props;
        const qualityGateProps: QualityGateWindowContainerProps = { 
            position: { left: '20%', top: '20%' }
        };

        return (
            <Container layout="vbox">
                <Container flex={1}>
                    <Container layout="vbox" docked="top" width="30%" minWidth="200" padding="10" defaults={{labelAlign: 'left'}}>
                        <ComboBoxField label="Application"  {...this.applicationData.buildConfig()}/>
                        <ComboBoxField label="Cost blocks" {...this.costBlockData.buildConfig()}/>
                        <ComboBoxField label="Cost elements"  {...this.costElementData.buildConfig()}/>
                        {
                            isVisibleDependencyItems 
                                ? <ComboBoxField key="dependencies" label="Dependencies"{...this.dependencyData.buildConfig()}/>
                                : <div/>
                        }
                        {
                            isVisibleRegions 
                                ? <ComboBoxField key="regions" label="Regions"{...this.regionData.buildConfig()}/>
                                : <div/>
                        }
                        <FileField label="Excel file" ref={button => this.fileField = button} onChange={this.onFileSelect}/>
                    </Container>

                    <Toolbar layout="hbox" docked="bottom">
                        <Button text="Import" disabled={!isImportButtonEnabled} handler={this.onImport} flex={1}/>
                    </Toolbar>                    
                </Container>

                <Grid store={this.resultStore} sortable={false} grouped={false} flex={1}>
                    <Column text="Status" dataIndex="info" flex={1}/>
                </Grid>

                <QualityGateWindowContainer {...qualityGateProps as any} />
            </Container>
        );
    }

    private createStore<T>(data: T[] = []): Store<T> {
        return Ext.create('Ext.data.Store', {
            data
        });
    }

    private buildComboboxChangeHandler(selector: SelectListSelector) {
        return (combobox, newValue, oldValue) => {
            const { onItemSelected } = selector(this.props);
            
            onItemSelected && onItemSelected(newValue);
        }
    }

    private buildComboboxData(selector: SelectListSelector): ComboboxData {
        const selectList = selector(this.props);
        const onChange = this.buildComboboxChangeHandler(selector);

        return {
            onChange,
            selector,
            buildConfig: () => {
                const selectList = selector(this.props);
                const store = this.createStore(selectList.list);
                const selection = store.getById(selectList.selectedItemId);

                return {
                    store,
                    onChange,
                    valueField: 'id',
                    displayField: 'name',
                    queryMode: 'local',
                    selection
                }
            }
        }
    }

    private updateStore<T>(prevItems: T[], nextItems: T[], store: Store<T>) {
        if (prevItems != nextItems) {
            store.loadData(nextItems);
        }
    }

    private onImport = () => {
        const { onImport } = this.props;
        const file = this.fileField.getFiles()[0];

        onImport && onImport(file);
    }

    private onFileSelect = (field, newValue, oldValue) => {
        const { onFileSelect } = this.props;

        onFileSelect && onFileSelect(newValue);
    }
}