<?php

use Illuminate\Database\Migrations\Migration;

class CreateSomePks extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::table('group_text', function($table) {
            $table->primary(array('text_id', 'group_id'));
        });
        
        Schema::table('group_user', function($table) {
            $table->primary(array('user_id', 'group_id'));
        });
        
        Schema::table('dictionaries', function($table) {
            $table->index('language_id');
        });
        
        Schema::table('tag_term', function($table) {
            $table->primary(array('tag_id', 'term_id'));
        });
        
        Schema::table('texts', function($table) {
            $table->index('l1_id');
            $table->index('title');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        //
    }

}
