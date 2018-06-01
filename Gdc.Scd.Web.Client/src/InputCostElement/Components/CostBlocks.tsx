import * as React from 'react';
import { Container, ComboBoxField, Panel, FormPanel, RadioField, ContainerField, Grid, Column, Toolbar, Button } from '@extjs/ext-react';
import { CostBlockInputState, EditItem, CheckItem } from '../States/CostBlock'
import { NamedId } from '../../Common/States/NamedId';
import { SelectList, MultiSelectList } from '../../Common/States/SelectList';
import { Filter } from './Filter';

Ext.require('Ext.grid.plugin.CellEditing');

export interface CostBlockActions {
  onCountrySelected(countryId: string);
}

export interface SelectListFilter {
  selectList: SelectList<NamedId>
  filter: CheckItem[],
  filterName: string
}

export interface CostBlockProps {
  country: SelectList<NamedId>,
  costElement: SelectListFilter & {
    description: string
  }
  inputLevel: SelectListFilter
  editItems: EditItem[]
}

export class CostBlock extends React.Component<CostBlockProps & CostBlockActions> {
  public render() {
    const { 
      country, 
      costElement: {
        selectList: costElements,
        filterName: costElFilterName,
        filter: costElFilter,
        description
      },
      inputLevel: {
        selectList: inputLevels
      } 
    } = this.props;

    return (
      <Container layout={{ type: 'hbox', align: 'stretch '}}>
        <Container flex={1} layout="vbox" shadow>
          <Container layout="hbox">
            <FormPanel flex={1}>
              {this.countryCombobox(country)}
              {this.radioFieldSet(costElements, 'Cost Elements')}
            </FormPanel>

            <Filter 
              title="Dependent from:" 
              //valueColumnText={costElFilterName}
              //items={costElFilter} 
              valueColumnText="Test title"
              height="500"
              items={[
                {id:'1', name:'test1', isChecked: true},
                {id:'2', name:'test2', isChecked: false},
                {id:'3', name:'test3', isChecked: false},
                {id:'4', name:'test4', isChecked: false},
                {id:'5', name:'test5', isChecked: false},
                {id:'6', name:'test7', isChecked: false},
              ]}
              flex={1} 
            />
          </Container>

          <Panel title="Description" padding="10">
            {description}
          </Panel>
        </Container>

        <Container flex={1} layout="vbox" padding="0px 0px 0px 5px">
          <Container layout="hbox">
            <FormPanel flex={1}>
              {this.radioFieldSet(inputLevels, 'Input Level')}
            </FormPanel>
            
            <Filter 
              //valueColumnText={costElFilterName}
              //items={costElFilter} 
              valueColumnText="Test title"
              height="350"
              items={[
                {id:'1', name:'test1', isChecked: true},
                {id:'2', name:'test2', isChecked: false},
                {id:'3', name:'test3', isChecked: false},
                {id:'4', name:'test4', isChecked: false},
                {id:'5', name:'test5', isChecked: false},
                {id:'6', name:'test7', isChecked: false},
              ]}
              flex={1} 
            />
          </Container>

          {this.editGrid([], 'Name title', 'Value title')}
        </Container>
      </Container>
    );
  }

  private countryCombobox(country: SelectList<NamedId>) {
    const countryStore = Ext.create('Ext.data.Store', {
        data: country.list
    });

    const selectedCountry = 
        countryStore.getData()
                    .findBy(item => (item.data as NamedId).id === country.selectedItemId);

    return (
        <ComboBoxField 
            label="Select a Country:"
            //width="50%"
            displayField="name"
            valueField="id"
            queryMode="local"
            store={countryStore}
            selection={selectedCountry}
            onChange={(combobox, newValue, oldValue) => this.props.onCountrySelected(newValue)}
        />
    );
  }

  private radioField(name: string, costElement: NamedId, selectedCostElementId: string) {
    return (
      <RadioField 
          key={costElement.id} 
          boxLabel={costElement.name} 
          name={name} 
          checked={costElement.id === selectedCostElementId}
      />
    );
  }

  private radioFieldSet(selectList: SelectList<NamedId>, label: string) {
    return (
      <ContainerField label={label} layout={{type: 'vbox', align: 'left'}}>
        {
          selectList && 
          selectList.list.map(costElement => 
            this.radioField('costElement', costElement, selectList.selectedItemId))
        }
      </ContainerField>
    );
  }

  private editGrid(items: EditItem[], nameTitle: string, valueTitle) {
    const store = Ext.create('Ext.data.Store', {
        //data: items
        data: [
          {id:'1', name:'test1', value: 10},
          {id:'2', name:'test2', value: 20},
          {id:'3', name:'test3', value: 30},
          {id:'4', name:'test4', value: 40},
          {id:'5', name:'test5', value: 50},
          {id:'6', name:'test7', value: 60},
        ]
    });

    return (
      <Grid 
        store={store} 
        flex={1} 
        shadow 
        height={400}
        columnLines={true}
        //plugins={[{ type: 'cellediting', triggerEvent: 'singletap' }]}
        //plugins={['cellediting', 'selectionreplicator']}
        plugins={{
          selectionreplicator: true,
          //clipboard: true
        }}
        selectable={{
          rows: true,
          cells: true,
          columns: true,
          drag: true
        }}
      >
        <Column text={nameTitle} dataIndex="name" flex={1}/>
        <Column text={valueTitle} dataIndex="value" flex={1} editable={true}/>

        <Toolbar docked="bottom">
            <Button text="Clear" flex={1}/>
            <Button text="Save" flex={1}/>
        </Toolbar>
      </Grid>
    );
  }   
}
