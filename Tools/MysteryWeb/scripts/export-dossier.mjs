import { mkdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..', '..');
const args = process.argv.slice(2);
function option(name, fallback) {
  const index = args.indexOf(name);
  return index >= 0 ? args[index + 1] : fallback;
}

function inline(value) {
  return String(value ?? '').replaceAll('\\', '\\\\').replaceAll('"', '\\"').replaceAll('\n', ' ');
}

function linesFor(value, depth = 0) {
  const pad = '  '.repeat(depth);
  if (Array.isArray(value)) return value.length ? value.map((item) => `${pad}- ${typeof item === 'object' && item !== null ? '\n' + linesFor(item, depth + 1) : inline(item)}`).join('\n') : `${pad}- (vazio)`;
  if (value && typeof value === 'object') return Object.entries(value).map(([key, item]) => `${pad}- **${key}**: ${typeof item === 'object' && item !== null ? '\n' + linesFor(item, depth + 1) : inline(item)}`).join('\n');
  return `${pad}${inline(value)}`;
}

function dossier(data) {
  const code = (value) => `\`${value}\``;
  const sections = [`# Dossiê narrativo — ${data.title ?? data.id}\n\nFonte estruturada: ${code(data.id)}. Este arquivo é exportado do case.json; IDs e atribuições permanecem rastreáveis.`];
  if (data.characters?.length) sections.push(`## Personagens\n\n${data.characters.map((item) => `### ${item.name ?? item.id} (${code(item.id)})\n\n${linesFor(item)}`).join('\n\n')}`);
  if (data.evidence?.length) sections.push(`## Evidências\n\n${data.evidence.map((item) => `### ${item.title ?? item.id} (${code(item.id)})\n\n- **Origem informacional:** ${code(item.origin_id ?? 'não informada')}\n- **Fonte imediata:** ${item.source?.label ?? item.source?.id ?? 'não informada'}\n\n${linesFor(item)}`).join('\n\n')}`);
  if (data.claims?.length) sections.push(`## Afirmações\n\n${data.claims.map((item) => `### ${item.text ?? item.id} (${code(item.id)})\n\n${linesFor(item)}`).join('\n\n')}`);
  if (data.interactions?.length) sections.push(`## Interações e diálogos\n\n${data.interactions.map((item) => `### ${item.title ?? item.id} (${code(item.id)})\n\n${linesFor(item)}`).join('\n\n')}`);
  if (data.hypotheses?.length) sections.push(`## Hipóteses e sustentação\n\n${data.hypotheses.map((item) => `### ${item.text ?? item.id} (${code(item.id)})\n\n${linesFor(item)}`).join('\n\n')}`);
  if (data.scenarios?.length) sections.push(`## Cenários de validação\n\n${data.scenarios.map((item) => `### ${item.title ?? item.id} (${code(item.id)})\n\n${linesFor(item)}`).join('\n\n')}`);
  sections.push(`## Caso completo (JSON)\n\nO bloco abaixo preserva integralmente todos os campos autorais, inclusive atributos adicionais que não tenham seção própria.\n\n\u0060\u0060\u0060json\n${JSON.stringify(data, null, 2)}\n\u0060\u0060\u0060`);
  return sections.join('\n\n---\n\n') + '\n';
}

function clueMap(data) {
  const rows = ['flowchart LR'];
  const add = (id, text, shape = 'rect') => {
    const node = shape === 'round' ? `${id}("${inline(text)}")` : `${id}["${inline(text)}"]`;
    rows.push(`  ${node}`);
  };
  const sources = new Set();
  const origins = new Set();
  for (const item of data.claims ?? []) add(`cl_${item.id}`, `${item.id} · ${item.text ?? item.id}`, 'round');
  for (const item of data.evidence ?? []) {
    add(`ev_${item.id}`, `${item.id} · ${item.title ?? item.id} · origem ${item.origin_id ?? '?'}`);
    if (item.origin_id && !origins.has(item.origin_id)) {
      origins.add(item.origin_id);
      add(`or_${item.origin_id}`, `Origem ${item.origin_id}`);
    }
    if (item.origin_id) rows.push(`  ev_${item.id} -. proveniência .-> or_${item.origin_id}`);
    if (item.source?.id) {
      const sourceKey = `${item.source.kind ?? 'source'}_${item.source.id}`;
      if (!sources.has(sourceKey)) {
        sources.add(sourceKey);
        add(sourceKey, `${item.source.kind ?? 'fonte'} · ${item.source.label ?? item.source.id}`);
      }
      rows.push(`  ${sourceKey} --> ev_${item.id}`);
    }
    for (const claimId of item.claimIds ?? []) rows.push(`  ev_${item.id} --> cl_${claimId}`);
  }
  for (const item of data.interactions ?? []) {
    add(`ix_${item.id}`, `${item.id} · ${item.title ?? item.id}${item.condition ? ` · condição ${JSON.stringify(item.condition)}` : ''}`);
    for (const line of item.lines ?? []) {
      const ids = line.effects?.evidence == null ? [] : Array.isArray(line.effects.evidence) ? line.effects.evidence : [line.effects.evidence];
      for (const id of ids) rows.push(`  ix_${item.id} --> ev_${id}`);
      for (const marker of line.effects?.setMarkers ?? []) rows.push(`  ix_${item.id} -. define marcador .-> mk_${marker}["Marcador ${inline(marker)}"]`);
      for (const marker of line.effects?.clearMarkers ?? []) rows.push(`  ix_${item.id} -. remove marcador .-> mk_${marker}["Marcador ${inline(marker)}"]`);
    }
    const inspected = item.evidenceId ?? item.evidence_id;
    if (inspected) rows.push(`  ix_${item.id} --> ev_${inspected}`);
  }
  for (const hypothesis of data.hypotheses ?? []) {
    add(`hy_${hypothesis.id}`, `${hypothesis.id} · ${hypothesis.text ?? hypothesis.id}`, 'round');
    for (const [index, alternative] of (hypothesis.alternatives ?? []).entries()) {
      const altId = `alt_${hypothesis.id}_${index + 1}`;
      add(altId, `Alternativa ${index + 1} · mín. ${alternative.minOrigins} origens`);
      rows.push(`  ${altId} --> hy_${hypothesis.id}`);
      for (const id of alternative.evidenceIds ?? []) rows.push(`  ev_${id} --> ${altId}`);
    }
    for (const id of hypothesis.contradictions ?? []) rows.push(`  ev_${id} -. contradição .-> hy_${hypothesis.id}`);
  }
  return rows.join('\n') + '\n';
}

const casePath = path.resolve(option('--case', path.join(root, 'Tools/MysteryWeb/data/case.json')));
const outDir = path.resolve(option('--out', path.join(root, 'Source/Narrative/mvp-arrival')));
const data = JSON.parse(await readFile(casePath, 'utf8'));
await mkdir(outDir, { recursive: true });
await Promise.all([
  writeFile(path.join(outDir, 'dossier.md'), dossier(data)),
  writeFile(path.join(outDir, 'clue-map.mmd'), clueMap(data)),
]);
process.stdout.write(`Dossiê e mapa exportados de ${casePath} para ${outDir}.\n`);
