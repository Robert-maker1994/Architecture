import { Request, Response } from 'express';
import { CreateTodoDto } from '../../application/dtos/create-todo.dto';
import { ITodo } from '../../domain/interfaces/ITodo';
import { TodoApplicationService } from '../../application/service/todo.service';

export class TodoController {
  constructor(private todoAppService: TodoApplicationService) {}

  // POST /todos
  async createTodo(req: Request, res: Response): Promise<void> {
    try {
      const createTodoDto: CreateTodoDto = req.body;
      if (!createTodoDto.text || typeof createTodoDto.text !== 'string' || createTodoDto.text.trim() === '') {
        res.status(400).json({ message: 'Todo text is required and must be a non-empty string.' });
        return;
      }
      const newTodo = await this.todoAppService.addTodo(createTodoDto);
      res.status(201).json(newTodo);
    } catch (error) {
      const err = error as Error;
      if (err.message.includes("cannot be empty")) {
          res.status(400).json({ message: err.message });
      } else {
          res.status(500).json({ message: 'Failed to create todo', error: err.message });
      }
    }
  }

  // GET /todos
  async getTodos(req: Request, res: Response): Promise<void> {
    try {
      const todos = await this.todoAppService.getAllTodos();
      res.status(200).json(todos);
    } catch (error) {
      const err = error as Error;
      res.status(500).json({ message: 'Failed to retrieve todos', error: err.message });
    }
  }

  // GET /todos/:id
  async getTodoById(req: Request, res: Response): Promise<void> {
    try {
      const todo = await this.todoAppService.getTodoById(req.params.id);
      if (todo) {
        res.status(200).json(todo);
      } else {
        res.status(404).json({ message: 'Todo not found' });
      }
    } catch (error) {
      const err = error as Error;
      res.status(500).json({ message: 'Failed to retrieve todo', error: err.message });
    }
  }

  // PUT /todos/:id
  async updateTodo(req: Request, res: Response): Promise<void> {
    try {
      const updates: Partial<Omit<ITodo, 'id' | 'createdAt'>> = req.body;
       if (updates.text !== undefined && (typeof updates.text !== 'string' || updates.text.trim() === '')) {
        res.status(400).json({ message: 'Todo text cannot be empty if provided for update.' });
        return;
      }
      if (updates.completed !== undefined && typeof updates.completed !== 'boolean') {
        res.status(400).json({ message: 'Completed status must be a boolean.' });
        return;
      }

      const updatedTodo = await this.todoAppService.updateTodo(req.params.id, updates);
      if (updatedTodo) {
        res.status(200).json(updatedTodo);
      } else {
        res.status(404).json({ message: 'Todo not found for update' });
      }
    } catch (error) {
      const err = error as Error;
       if (err.message.includes("cannot be updated to empty")) {
          res.status(400).json({ message: err.message });
      } else {
        res.status(500).json({ message: 'Failed to update todo', error: err.message });
      }
    }
  }
  
  // PATCH /todos/:id/toggle
  async toggleTodo(req: Request, res: Response): Promise<void> {
    try {
        const toggledTodo = await this.todoAppService.toggleTodoCompletion(req.params.id);
        if (toggledTodo) {
            res.status(200).json(toggledTodo);
        } else {
            res.status(404).json({ message: 'Todo not found to toggle completion' });
        }
    } catch (error) {
        const err = error as Error;
        res.status(500).json({ message: 'Failed to toggle todo completion', error: err.message });
    }
  }


  // DELETE /todos/:id
  async deleteTodo(req: Request, res: Response): Promise<void> {
    try {
      const success = await this.todoAppService.removeTodo(req.params.id);
      if (success) {
        res.status(204).send(); // No content
      } else {
        res.status(404).json({ message: 'Todo not found for deletion' });
      }
    } catch (error) {
      const err = error as Error;
      res.status(500).json({ message: 'Failed to delete todo', error: err.message });
    }
  }
}