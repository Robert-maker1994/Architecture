
import { ITodo, ITodoRepository } from '../../domain/interfaces/ITodo';
import { DomainValidation } from '../../domain/todo.logic';
import { CreateTodoDto } from '../dtos/create-todo.dto';
import { TodoDto } from '../dtos/todo.dto';

export class TodoApplicationService {
  // Dependency Injection for the repository
  constructor(private todoRepository: ITodoRepository, private todoLogic: DomainValidation) {}

  async addTodo(createTodoDto: CreateTodoDto): Promise<TodoDto> {
    
    if (!createTodoDto.text || createTodoDto.text.trim() === '') {
      throw new Error("Todo text cannot be empty for creation.");
    }

    if (this.todoLogic.validateText(createTodoDto.text)) {
      throw new Error('Invalid todo text.');
    }

    const newTodoDomainObject = await this.todoRepository.create({ text: createTodoDto.text });
    return new TodoDto(newTodoDomainObject); // Map to DTO for presentation
  }

  async getTodoById(id: string): Promise<TodoDto | null> {
    const todoDomainObject = await this.todoRepository.findById(id);
    return todoDomainObject ? new TodoDto(todoDomainObject) : null;
  }

  async getAllTodos(): Promise<TodoDto[]> {
    const todoDomainObjects = await this.todoRepository.findAll();
    return todoDomainObjects.map((todo: ITodo) => new TodoDto(todo));
  }

  async updateTodo(id: string, updates: Partial<Omit<ITodo, 'id' | 'createdAt'>>): Promise<TodoDto | null> {
    const updatedTodoDomainObject = await this.todoRepository.update(id, updates);
    return updatedTodoDomainObject ? new TodoDto(updatedTodoDomainObject) : null;
  }

  async removeTodo(id: string): Promise<boolean> {
    return this.todoRepository.delete(id);
  }

  async toggleTodoCompletion(id: string): Promise<TodoDto | null> {
    const todo = await this.todoRepository.findById(id);
    if (!todo) {
        return null;
    }
    const updatedTodo = await this.todoRepository.update(id, { completed: !todo.completed });
    return updatedTodo ? new TodoDto(updatedTodo) : null;
  }
}

