import * as React from 'react';
import { Container, ComboBoxField, Panel, FormPanel, RadioField, ContainerField, Grid, Column, Toolbar, Button, Label, Dialog } from '@extjs/ext-react';
import { CostBlockState, EditItem, CheckItem } from '../States/CostBlockStates'
import { Filter } from './Filter';
import { SelectList, NamedId } from '../../Common/States/CommonStates';
import { EditGridToolProps, EditGridTool, EditGridToolActions } from './EditGridTool';

Ext.require([
  'Ext.grid.plugin.CellEditing', 
]);

export interface CostBlockActions {
  onRegionSelected?: (regionId: string) => void
  onCostElementSelected?: (costElementId: string) => void
  onCostElementFilterSelectionChanged?: (
    costElementId: string, 
    filterItemId: string,
    isSelected: boolean) => void
  onCostElementFilterReseted?: (costElementId: string) => void
  onInputLevelSelected?: (costElementId: string, inputLevelId: string) => void
  onInputLevelFilterSelectionChanged?: (
    costElementId: string,
    inputLevelId: string, 
    filterItemId: string,
    isSelected: boolean) => void
  onInputLevelFilterReseted?: (costElementId: string, inputLevelId: string) => void
  onEditItemsCleared?: () => void
  onItemEdited?: (item: EditItem) => void
  onEditItemsSaving?: (forApproval: boolean) => void
  onApplyFilters?: () => void
}

export interface SelectListFilter {
  id: string
  selectList: SelectList<NamedId>
  filter?: CheckItem[],
  filterName?: string,
  isVisibleFilter: boolean
  isEnableList: boolean
}

export interface CostElementProps extends SelectListFilter {
  description?: string
}

export interface RegionProps {
  selectedList: SelectList<NamedId>
  name: string
  isEnabled: boolean
}

export interface CostBlockProps {
  region: RegionProps
  costElement: CostElementProps
  inputLevel: SelectListFilter
  edit: EditGridToolProps
}

export class CostBlockView extends React.Component<CostBlockProps & CostBlockActions> {
  public render() {
    const { 
      costElement, 
      inputLevel,
      onCostElementSelected,
      onInputLevelSelected,
      onCostElementFilterSelectionChanged,
      onInputLevelFilterSelectionChanged,
      onCostElementFilterReseted,
      onInputLevelFilterReseted,
      edit
    } = this.props;

    const editActions: EditGridToolActions = {
      onApplyFilters: this.props.onApplyFilters,
      onCleared: this.props.onEditItemsCleared,
      onItemEdited: this.props.onItemEdited,
      onSaving: this.props.onEditItemsSaving
    }

    return (
      <Container layout={{ type: 'hbox', align: 'stretch '}}>
        <Container flex={1} layout="vbox" shadow>
          <Container layout="hbox" flex={3}>
            <FormPanel flex={1}>
              {this.regionCombobox()}
              {
                this.radioFieldSet(
                  `${costElement.id}_costelements`, 
                  costElement.selectList, 
                  'Cost Elements', 
                  costElement.isEnableList,
                  costElement => onCostElementSelected && onCostElementSelected(costElement.id)
                )
              }
            </FormPanel>

            { 
              costElement.isVisibleFilter &&
              <Filter 
                title="Dependent from:" 
                valueColumnText={costElement.filterName}
                items={costElement.filter} 
                flex={1}
                onSelectionChanged={
                  (item: NamedId, isSelected: boolean) =>
                    onCostElementFilterSelectionChanged &&
                    onCostElementFilterSelectionChanged(
                      costElement.selectList.selectedItemId,
                      item.id,
                      isSelected
                    )
                }
                onReset={
                  () => 
                    onCostElementFilterReseted && 
                    onCostElementFilterReseted(costElement.selectList.selectedItemId)
                }
              />
            }
          </Container>

          <Panel title="Description" padding="10" scrollable flex={1}>
            {
              costElement.description != null &&
              <Label html={costElement.description}/>
            }
          </Panel>
        </Container>

        <Container flex={1} layout="vbox" padding="0px 0px 0px 5px">
          {
            inputLevel &&
            <Container layout="hbox" flex={1}>
              <FormPanel flex={1}>
                {
                  this.radioFieldSet(
                    `${inputLevel.id}_inputlevels`, 
                    inputLevel.selectList, 
                    'Input Level',
                    inputLevel.isEnableList,
                    inputLevel => onInputLevelSelected && onInputLevelSelected(costElement.selectList.selectedItemId, inputLevel.id)
                  )
                }
              </FormPanel>
              
              {
                inputLevel.isVisibleFilter &&
                <Filter 
                  valueColumnText={inputLevel.filterName}
                  items={inputLevel.filter} 
                  flex={1}
                  onSelectionChanged={
                    (item: NamedId, isSelected: boolean) =>
                      onInputLevelFilterSelectionChanged &&
                      onInputLevelFilterSelectionChanged(
                        costElement.selectList.selectedItemId,
                        inputLevel.selectList.selectedItemId,
                        item.id,
                        isSelected
                      )
                  }
                  onReset={
                    () => onInputLevelFilterReseted && 
                          onInputLevelFilterReseted(
                            costElement.selectList.selectedItemId, 
                            inputLevel.selectList.selectedItemId)
                  }
                />
              }
            </Container>
          }

          {
            edit && 
            <EditGridTool {...edit} {...editActions} flex={2}/>
          }
        </Container>
      </Container>
    );
  }

  private regionCombobox() {
    let result;

    const { region } = this.props;

    if (region) {
      const regionStore = Ext.create('Ext.data.Store', {
          data: region.selectedList ? region.selectedList.list : []
      });

      const selectedRegion = 
          regionStore.getData()
                      .findBy(item => (item.data as NamedId).id === region.selectedList.selectedItemId);

      result = (
          <ComboBoxField 
              disabled={!region.isEnabled}
              label={region.name}
              displayField="name"
              valueField="id"
              queryMode="local"
              store={regionStore}
              selection={selectedRegion}
              onChange={(combobox, newValue, oldValue) => this.props.onRegionSelected(newValue)}
          />
      );
    }

    return result
  }

  private radioField(
    name: string, 
    item: NamedId, 
    selectedCostElementId: string,
    isEnabled: boolean,
    onSelected: (item: NamedId) => void
  ) {
    return (
      <RadioField 
          key={`${name}_${item.id}`} 
          boxLabel={item.name} 
          name={name} 
          checked={item.id === selectedCostElementId}
          disabled={!isEnabled}
          onCheck={
            radioField => {
              //HACK: onCheck event fired twice.
              if ((radioField as any).hasFocus) {
                onSelected(item);
              }

              return false;
            }
          }
      />
    );
  }

  private radioFieldSet(
    setName: string, 
    selectList: SelectList<NamedId>, 
    label: string, 
    isEnable: boolean,
    onSelected: (item: NamedId) => void
  ) {    
    return (
      <ContainerField label={label} layout={{type: 'vbox', align: 'left'}} scrollable maxHeight="500px">
        {
          selectList && 
          selectList.list &&
          selectList.list.map(item => 
            this.radioField(
              setName, 
              item, 
              selectList.selectedItemId,
              isEnable,
              onSelected
            )
          )
        }
      </ContainerField>
    );
  }
}
