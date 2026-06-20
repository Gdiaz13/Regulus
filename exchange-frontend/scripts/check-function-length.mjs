import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import ts from 'typescript';

const maxLines = 15;
const scriptRoot = path.dirname(fileURLToPath(import.meta.url));
const frontendRoot = path.resolve(scriptRoot, '..');
const workspaceRoot = path.resolve(frontendRoot, '..');

main();

function main() {
  const findings = [...scanTypeScriptProject(), ...scanCSharpProject()];
  if (findings.length === 0) {
    console.log(`No functions exceed ${maxLines} lines.`);
    return;
  }
  findings.map(formatFinding).forEach((finding) => console.log(finding));
  process.exitCode = 1;
}

function scanTypeScriptProject() {
  return filesUnder(path.join(frontendRoot, 'src'), isTypeScriptFile).flatMap(scanTypeScriptFile);
}

function scanCSharpProject() {
  return filesUnder(path.join(workspaceRoot, 'api'), isCSharpFile).flatMap(scanCSharpFile);
}

function filesUnder(root, predicate) {
  return fs.readdirSync(root, { withFileTypes: true }).flatMap((entry) => {
    const fullPath = path.join(root, entry.name);
    if (entry.isDirectory()) {
      return filesUnder(fullPath, predicate);
    }
    return predicate(fullPath) ? [fullPath] : [];
  });
}

function scanTypeScriptFile(file) {
  const source = createSourceFile(file);
  const findings = [];
  inspectTypeScriptNode(source, source, file, findings);
  return findings;
}

function createSourceFile(file) {
  const text = fs.readFileSync(file, 'utf8');
  return ts.createSourceFile(file, text, ts.ScriptTarget.Latest, true, scriptKind(file));
}

function inspectTypeScriptNode(node, source, file, findings) {
  if (isFunctionLike(node)) {
    recordTypeScriptNode(node, source, file, findings);
  }
  ts.forEachChild(node, (child) => inspectTypeScriptNode(child, source, file, findings));
}

function recordTypeScriptNode(node, source, file, findings) {
  const start = sourceLine(source, node.getStart(source));
  const end = sourceLine(source, node.getEnd());
  const lines = end - start + 1;
  if (lines > maxLines) {
    findings.push(finding(file, functionName(node), start, lines));
  }
}

function scanCSharpFile(file) {
  const lines = fs.readFileSync(file, 'utf8').split(/\r?\n/);
  return lines.flatMap((_, index) => cSharpFinding(file, lines, index));
}

function cSharpFinding(file, lines, index) {
  if (!isMethodStart(lines[index])) {
    return [];
  }
  const end = blockEnd(lines, index);
  return cSharpMethodFinding(file, index, end);
}

function cSharpMethodFinding(file, start, end) {
  const lines = end === null ? 0 : end - start + 1;
  return lines > maxLines ? [finding(file, 'method', start + 1, lines)] : [];
}

function blockEnd(lines, start) {
  let depth = 0;
  let seenBody = false;
  for (let index = start; index < lines.length; index += 1) {
    depth += braceCount(lines[index], '{');
    seenBody = seenBody || depth > 0;
    depth -= braceCount(lines[index], '}');
    if (seenBody && depth === 0) {
      return index;
    }
  }
  return null;
}

function isFunctionLike(node) {
  return ts.isFunctionDeclaration(node)
    || ts.isFunctionExpression(node)
    || ts.isArrowFunction(node)
    || ts.isMethodDeclaration(node);
}

function functionName(node) {
  if (node.name?.text) {
    return node.name.text;
  }
  return node.parent?.name?.text ?? '<anonymous>';
}

function sourceLine(source, position) {
  return source.getLineAndCharacterOfPosition(position).line + 1;
}

function scriptKind(file) {
  return file.endsWith('.tsx') ? ts.ScriptKind.TSX : ts.ScriptKind.TS;
}

function isTypeScriptFile(file) {
  return /\.tsx?$/.test(file);
}

function isCSharpFile(file) {
  return file.endsWith('.cs');
}

function isMethodStart(line) {
  return /^\s*(public|private|internal|protected)\s+(static\s+)?(override\s+)?(async\s+)?[\w<>?[\],\s]+\s+\w+\s*\(/.test(line);
}

function braceCount(line, brace) {
  return [...line].filter((character) => character === brace).length;
}

function finding(file, name, start, lines) {
  return { file: label(file), name, start, lines };
}

function label(file) {
  return path.relative(workspaceRoot, file).replaceAll(path.sep, '/');
}

function formatFinding({ file, name, start, lines }) {
  return `${file}:${start} ${name} has ${lines} lines.`;
}
