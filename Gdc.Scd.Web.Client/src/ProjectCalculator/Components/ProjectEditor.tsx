import * as React from "react";
import { Container, FormPanel, TextField, Grid, Column, Toolbar, Button, DatePickerField } from "@extjs/ext-react";
import { Project, ProjectItem } from "../States/Project";
import { Store } from "../../Common/States/ExtStates";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { ProjectItemEditData } from "../States/ProjectCalculatorState";
import { NamedId } from "../../Common/States/CommonStates";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { LocalDynamicGrid } from "../../Common/Components/LocalDynamicGrid";
import { ProjectItemsGrid } from "./ProjectItemsGrid";

export interface ProjectEditorActions {
    onInit?()
    onBackToList?(project: Project)
    onSave?(project: Project)
}

export interface ProjectEditorProps extends ProjectEditorActions {
    project: Project
    projectItemEditData: ProjectItemEditData    
}

export interface ProjectEditorState {
    saveButtonDisabled: boolean
}

export class ProjectEditor extends React.PureComponent<ProjectEditorProps, ProjectEditorState> {
    // private form
    private projectItemGrid: ProjectItemsGrid
    private projectNameField
    // private projectItemStore: Store<ProjectItem> = this.createProjectItemStore()

    constructor(props: ProjectEditorProps) {
        super(props);

        this.state = {
            saveButtonDisabled: true
        }
    }

    public componentDidMount() {
        const { onInit } = this.props;

        onInit && onInit();
    }

    public render() {
        const project = this.props.project || {} as Project;

        return (
            <FormPanel layout="vbox">
                {/* <DatePickerField label="Creation date" value={project.creationDate} readOnly={true}/> */}
                <TextField label="User" value={project.user && project.user.name} readOnly={true}/>
                <TextField ref={this.setProjectNameFieldRef} label="Project name" value={project.name} onChange={this.onProjectChanged}/>
                
                <ProjectItemsGrid 
                    ref={this.setProjectItemsGrid} 
                    projectItemEditData={this.props.projectItemEditData}
                    projectItems={project.projectItems || []}
                    onUpdateRecord={this.onProjectChanged}
                />

                <Toolbar layout="hbox" docked="bottom">
                    <Button text="Back To List" handler={this.onBackToList} flex={1}/>
                    <Button text="Save" handler={this.onSave} flex={1} disabled={this.state.saveButtonDisabled}/>
                </Toolbar>  
            </FormPanel>
        )
    }

    // private setFormRef = form => {
    //     this.form = form
    // }

    private setProjectItemsGrid = grid => {
        this.projectItemGrid = grid;
    }

    private setProjectNameFieldRef = field => {
        this.projectNameField = field;
    }

    private onProjectChanged = () => {
        this.setState({ saveButtonDisabled: false })
    }

    private onBackToList = (project: Project) => {
        const { onBackToList } = this.props;

        onBackToList && onBackToList(project);
    }

    private onSave = () => {
        Ext.Msg.confirm(
            'Save Project', 
            'Do you want to save the project?',
            (buttonId: string) => {
                if (buttonId == 'yes') {
                    const { onSave } = this.props;

                    onSave && onSave(this.getEditedProject());
                }
            },
            this
        );
    }

    private getEditedProject = () => {
        return {
            ...this.props.project,
            name: this.projectNameField.getValue(),
            projectItems: this.projectItemGrid.getEditedProjectItems()
        } as Project
    }
}