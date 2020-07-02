import * as React from "react";
import { Grid, Column, DateColumn, Toolbar, Button, Menu, MenuItem } from "@extjs/ext-react";
import { Store, Model } from "../../Common/States/ExtStates";
import { Project } from "../States/Project";
import { NamedId } from "../../Common/States/CommonStates";

const DataIndexes = {
    name: 'name',
    user: 'user',
    creationDate: 'creationDate'
}

export interface ProjectListActions {
    onAdd?(store: Store<Project>)
    onEdit?(store: Store<Project>, selectedProject: Project)
    onDelete?(store: Store<Project>, selectedProject: Project)
    onInit?()
    onSelectProject?(selectedProject: Project)
    onReportClick?(reportName: string, selectedProject: Project)
}

export interface ProjectListProps extends ProjectListActions {
    url: string
    selectedProject: Project
}

export class ProjectList extends React.PureComponent<ProjectListProps> {
    private readonly store: Store<Project>
    private readonly selectable = { mode: 'single' }
    private readonly reportMenuDefaults

    constructor(prop: ProjectListProps) {
        super(prop)

        this.store = this.createStore(prop.url);
        this.reportMenuDefaults = {
            handler: this.onReportClick
        }
    }

    public render() {
        const disabled = !this.props.selectedProject;

        return (
            <Grid  
                store={this.store} 
                selectable={this.selectable} 
                onSelect={this.onSelect} 
                masked={{ xtype: "loadmask" }}
                emptyText="No projects is available..."
                columnLines={true} 
                border={true}
                plugins={['pagingtoolbar']}
            >
                <DateColumn text="CreationDate" dataIndex={DataIndexes.creationDate} format="Y-m-d H:i:s" flex={1}/>
                <Column text="Name" dataIndex={DataIndexes.name} flex={1}/>
                <Column text="User" dataIndex={DataIndexes.user} renderer={this.userRenderer} flex={1}/>

                <Toolbar layout="hbox" docked="top">
                    <Button text="Add" handler={this.onAdd} flex={1}/>
                    <Button text="Edit" handler={this.onEdit} flex={1} disabled={disabled}/>
                    <Button text="Delete" handler={this.onDelete} flex={1} disabled={disabled}/>      

                    <Button text="Reports" disabled={disabled}>
                        <Menu defaults={this.reportMenuDefaults}>
                            <MenuItem text="LOCAP reports (for a specific country)" value="Project-Calc-Locap" />
                            <MenuItem text="LOCAP reports(approved)" value="Project-Calc-Locap-Approved" />
                            <MenuItem text="LOCAP reports detailed" value="Project-Calc-Locap-Detailed" />
                            <MenuItem text="LOCAP reports detailed(approved)" value="Project-Calc-Locap-Detailed-Approved" />
                            <MenuItem text="Contract reports" value="Project-Calc-Contract" />
                            <MenuItem text="Calculation Parameter Overview reports for HW maintenance cost elements" value="Project-Calc-Param-hw" />
                            <MenuItem text="Calculation Parameter Overview reports for HW maintenance cost elements (not approved)" value="Project-Calc-Param-hw-not-approved" />
                        </Menu>
                    </Button>                                  
                </Toolbar>  
            </Grid>
        )
    }

    public componentDidMount() {
        const { onInit } = this.props;

        onInit && onInit();
    }

    private onReportClick = ({ value }) => {
        const { selectedProject, onReportClick } = this.props;

        onReportClick && onReportClick(value, selectedProject);
    }

    private createStore(url: string): Store<Project> {
        return Ext.create('Ext.data.Store', {
            fields: [ 
                { name: DataIndexes.name, type: 'string' },
                { name: DataIndexes.user },
                { name: DataIndexes.creationDate, type: 'date' },
            ],
            autoLoad: true,
            remoteSort: true,
            pageSize : 100,
            proxy: {
                type: 'ajax',
                url,
                reader: { 
                    type: 'json',
                    rootProperty: 'items',
                    totalProperty: 'total'
                }
            }
        });
    }

    private userRenderer(user: NamedId) {
        return user && user.name
    }

    private onSelect = (grid, selectedItem: Model<Project>) => {
        const { onSelectProject } = this.props;

        onSelectProject && onSelectProject(selectedItem.data)
    }

    private onAdd = () => {
        const { onAdd } = this.props;

        onAdd && onAdd(this.store);
    }

    private onEdit = () => {
        const { onEdit, selectedProject } = this.props;

        onEdit && onEdit(this.store, selectedProject)
    }

    private onDelete = () => {
        const { onDelete, selectedProject } = this.props;

        onDelete && onDelete(this.store, selectedProject);
    }
}