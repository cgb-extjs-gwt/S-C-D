import * as React from "react";

export interface AccordionProps {
    title: React.ReactNode;
}

export class Accordion extends React.Component<AccordionProps, any> {

    private header: HTMLElement;

    private body: HTMLElement;

    public render() {
        return (
            <div className="accordion-block">
                <div ref={x => this.header = x} className="accordion" onClick={x => this.onHeaderClick()}>
                    {this.props.title}
                </div>
                <div ref={x => this.body = x} className="accordion-panel">
                    {this.props.children}
                </div>
            </div>
        );
    }

    private onHeaderClick() {
        this.header.classList.toggle("accordion-active");

        let body = this.body;
        if (body.style.maxHeight) {
            body.style.maxHeight = null;
        } else {
            body.style.maxHeight = body.scrollHeight + "px";
        }
    }
}