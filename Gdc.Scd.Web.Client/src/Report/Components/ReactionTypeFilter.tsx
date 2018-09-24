import { DictFilter, DictFilterProps } from "./DictFilter";

export class ReactionTypeFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getReactionTypes()
        });
    }
}