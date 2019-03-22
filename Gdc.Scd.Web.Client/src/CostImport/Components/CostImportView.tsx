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
    inputLevels: SelectListAdvanced<NamedId>
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
    autoSelectLastItem: boolean
    buildConfig(): {
        store: Store<NamedId>
        selection: Model<NamedId>
        displayField: string
        valueField: string
        onChange(combobox, newValue, oldValue)
    }
}

export class CostImportView extends React.PureComponent<CostImportViewProps> {
    private readonly applicationData = this.buildComboboxData(props => props.applications)
    private readonly costBlockData = this.buildComboboxData(props => props.costBlocks)
    private readonly costElementData = this.buildComboboxData(props => props.costElements)
    private readonly inputLevelData = this.buildComboboxData(props => props.inputLevels, true)
    private readonly dependencyData = this.buildComboboxData(props => props.dependencyItems)
    private readonly regionData = this.buildComboboxData(props => props.regions)
    private readonly allComboboxData = [
        this.applicationData,
        this.costBlockData,
        this.costElementData,
        this.inputLevelData,
        this.dependencyData,
        this.regionData
    ]
    private resultStore: Store<ResultImportItem>
    private fileField

    constructor(props: CostImportViewProps) {
        super(props)
        this.resultStore = this.createStore(props.resultImport);

        this.autoComboboxSelect(this.props);
    }

    public componentWillReceiveProps(nextProps: CostImportViewProps) {
        this.updateStore(this.props.resultImport, nextProps.resultImport, this.resultStore);
        this.autoComboboxSelect(nextProps);
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
                        <ComboBoxField label="Cost block" {...this.costBlockData.buildConfig()}/>
                        <ComboBoxField label="Cost element"  {...this.costElementData.buildConfig()}/>
                        {
                            isVisibleRegions 
                                ? <ComboBoxField key="regions" label="Region"{...this.regionData.buildConfig()}/>
                                : <div/>
                        }
                        {
                            isVisibleDependencyItems 
                                ? <ComboBoxField key="dependencies" label="Dependency"{...this.dependencyData.buildConfig()}/>
                                : <div/>
                        }
                        <ComboBoxField label="Input level"  {...this.inputLevelData.buildConfig()}/>
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

            onItemSelected && onItemSelected(newValue == "" ? null : newValue);
        }
    }

    private buildComboboxKeyUpHandler() {
        return (combo, e) => {
            let value = combo.getInputValue();

            if (e.keyCode != 38 && e.keyCode != 40) //arrow UP and DOWN
            {
                if (value && value.length > 0) {
                    combo.getStore().filterBy(record => record.data.name.toLowerCase().startsWith(value.toLowerCase()));
                }
            }
        }
    }

    private buildComboboxData(selector: SelectListSelector, autoSelectLastItem: boolean = false): ComboboxData {
        const onChange = this.buildComboboxChangeHandler(selector);
        const onKeyUp = this.buildComboboxKeyUpHandler();
        const onBlur = (combo, e) => { combo.getStore().clearFilter(false) }

        return {
            onChange,
            selector,
            autoSelectLastItem,
            buildConfig: () => {
                const { list, selectedItemId } = selector(this.props);
                const store = this.createStore(list);
                const selection = store.getById(selectedItemId);

                return {
                    store,
                    onChange,
                    valueField: 'id',
                    displayField: 'name',
                    queryMode: 'local',
                    selection,
                    clearable: true,
                    forceSelection: true,
                    onKeyUp,
                    onBlur
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

    private autoComboboxSelect(props: CostImportViewProps) {
        this.allComboboxData.forEach(data => {
            const { list, selectedItemId, onItemSelected } = data.selector(props);

            if (list && list.length > 0 && selectedItemId == null) {
                if (data.autoSelectLastItem) {
                    onItemSelected(list[list.length - 1].id);
                } else if (list.length == 1) {
                    onItemSelected(list[0].id);
                }
            }
        });
    }
}