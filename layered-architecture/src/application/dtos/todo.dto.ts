import { ITodo } from "../../domain/interfaces/ITodo";

export class TodoDto {
  id: string;
  text: string;
  completed: boolean;
  createdAt: string; 

  constructor(todo: ITodo) {
    this.id = todo.id;
    this.text = todo.text;
    this.completed = todo.completed;
    this.createdAt = todo.createdAt.toISOString();
  }
}