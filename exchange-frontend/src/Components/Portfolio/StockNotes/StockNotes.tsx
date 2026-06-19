import { useState } from 'react';
import type { FormEvent } from 'react';
import type { IStockComment } from '../../../Interfaces/APIResponses/IStockComment';
import { useStockComments } from '../../../hooks/useStockComments';
import styles from './StockNotes.module.css';

type Comments = ReturnType<typeof useStockComments>;

const StockNotes = ({ stockId }: { stockId: number }) => {
  const comments = useStockComments(stockId);
  const form = useNoteForm(comments.add);
  return (
    <section className={styles.notes}>
      <NoteForm {...form} />
      <NoteMessage comments={comments} />
      <NoteList comments={comments} />
    </section>
  );
};

function useNoteForm(add: Comments['add']) {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (canSubmit(title, content) && await add({ title, content })) {
      setTitle('');
      setContent('');
    }
  };
  return { title, setTitle, content, setContent, submit, submitLabel: 'Save note' };
}

function NoteForm(props: NoteFormProps) {
  return (
    <form className={styles.form} onSubmit={props.submit}>
      <NoteFields {...props} />
      <NoteActions {...props} />
    </form>
  );
}

function NoteFields(props: NoteFormProps) {
  return (
    <>
      <input value={props.title} onChange={(event) => props.setTitle(event.target.value)} placeholder="Note title" />
      <textarea value={props.content} onChange={(event) => props.setContent(event.target.value)} placeholder="What matters?" />
    </>
  );
}

function NoteActions(props: NoteFormProps) {
  return (
    <div className={styles.actions}>
      <button type="submit" disabled={!canSubmit(props.title, props.content)}>{props.submitLabel}</button>
      {props.onCancel && <button type="button" onClick={props.onCancel}>Cancel</button>}
    </div>
  );
}

function NoteMessage({ comments }: { comments: Comments }) {
  if (comments.status === 'loading') {
    return <p className={styles.message}>Loading notes...</p>;
  }
  return comments.status === 'error' ? <p className={styles.message}>{comments.message}</p> : null;
}

function NoteList({ comments }: { comments: Comments }) {
  if (comments.values.length === 0 && comments.status !== 'loading') {
    return <p className={styles.empty}>No notes yet.</p>;
  }
  return <ul className={styles.list}>{comments.values.map(renderNote(comments))}</ul>;
}

function renderNote(comments: Comments) {
  return (comment: IStockComment) => <NoteItem key={comment.id} comment={comment} comments={comments} />;
}

function NoteItem({ comment, comments }: NoteItemProps) {
  const [editing, setEditing] = useState(false);
  if (editing) {
    return <EditableNote comment={comment} comments={comments} onDone={() => setEditing(false)} />;
  }
  return <ReadOnlyNote comment={comment} comments={comments} onEdit={() => setEditing(true)} />;
}

function ReadOnlyNote({ comment, comments, onEdit }: ReadOnlyNoteProps) {
  return (
    <li className={styles.note}>
      <strong>{comment.title}</strong>
      <span>{formatDate(comment.createdOn)}</span>
      <p>{comment.content}</p>
      <div className={styles.actions}>
        <button type="button" onClick={onEdit}>Edit note</button>
        <button type="button" onClick={() => comments.remove(comment.id)}>Delete note</button>
      </div>
    </li>
  );
}

function EditableNote({ comment, comments, onDone }: EditableNoteProps) {
  const form = useEditNoteForm(comment, comments.update, onDone);
  return (
    <li className={styles.note}>
      <NoteForm {...form} />
    </li>
  );
}

function useEditNoteForm(comment: IStockComment, update: Comments['update'], onDone: () => void) {
  const [title, setTitle] = useState(comment.title);
  const [content, setContent] = useState(comment.content);
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (canSubmit(title, content) && await update(comment.id, { title, content })) {
      onDone();
    }
  };
  return { title, setTitle, content, setContent, submit, submitLabel: 'Update note', onCancel: onDone };
}

function canSubmit(title: string, content: string) {
  return title.trim().length > 0 && content.trim().length > 0;
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString();
}

type NoteFormProps = {
  title: string;
  setTitle: (value: string) => void;
  content: string;
  setContent: (value: string) => void;
  submit: (event: FormEvent<HTMLFormElement>) => void;
  submitLabel: string;
  onCancel?: () => void;
};

type NoteItemProps = {
  comment: IStockComment;
  comments: Comments;
};

type ReadOnlyNoteProps = NoteItemProps & {
  onEdit: () => void;
};

type EditableNoteProps = NoteItemProps & {
  onDone: () => void;
};

export default StockNotes;
